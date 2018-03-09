namespace IdentityBase.EntityFramework.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.Entities;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class TokenCleanupService
    {
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;
        private CancellationTokenSource _source;

        public TokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TokenCleanupService> logger,
            EntityFrameworkOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.TokenCleanupInterval < 1)
            {
                throw new ArgumentException(
                    "interval must be more than 1 second");
            }

            this._logger = logger ?? throw
                new ArgumentNullException(nameof(logger));

            this._serviceProvider = serviceProvider ?? throw
                new ArgumentNullException(nameof(serviceProvider));

            this._interval = TimeSpan.FromSeconds(options.TokenCleanupInterval);
        }

        public void Start()
        {
            this.Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (this._source != null)
            {
                throw new InvalidOperationException(
                    "Already started. Call Stop first.");
            }

            this._logger.LogDebug("Starting token cleanup");

            this._source = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternal(this._source.Token));
        }

        public void Stop()
        {
            if (this._source == null)
            {
                throw new InvalidOperationException(
                        "Not started. Call Start first.");
            }

            this._logger.LogDebug("Stopping token cleanup");

            this._source.Cancel();
            this._source = null;
        }

        private async Task StartInternal(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this._logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    await Task.Delay(this._interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    this._logger.LogDebug("TaskCanceledException. Exiting.");
                    break;
                }
                catch (Exception ex)
                {
                    this._logger.LogError(
                        "Task.Delay exception: {0}. Exiting.",
                        ex.Message);
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    this._logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                this.TryClearTokens();
            }
        }

        public void TryClearTokens()
        {
            try
            {
                this.ClearTokens();
            }
            catch (Exception ex)
            {
                this._logger.LogError(
                    "Exception clearing tokens: {exception}",
                    ex.Message);
            }
        }

        public void ClearTokens()
        {
            this._logger.LogTrace("Querying for tokens to clear");

            using (IServiceScope serviceScope = this._serviceProvider
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (IPersistedGrantDbContext context = serviceScope
                    .ServiceProvider.GetService<IPersistedGrantDbContext>())
                {
                    PersistedGrant[] expired = context.PersistedGrants
                        .Where(x => x.Expiration < DateTimeOffset.UtcNow)
                        .ToArray();

                    this._logger.LogInformation(
                        "Clearing {tokenCount} tokens",
                        expired.Length);

                    if (expired.Length > 0)
                    {
                        context.PersistedGrants.RemoveRange(expired);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}