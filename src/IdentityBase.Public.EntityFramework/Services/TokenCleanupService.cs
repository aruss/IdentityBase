namespace IdentityBase.Public.EntityFramework.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.Public.EntityFramework.Entities;
    using IdentityBase.Public.EntityFramework.Interfaces;
    using IdentityBase.Public.EntityFramework.Options;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    internal class TokenCleanupService
    {
        private readonly ILogger<TokenCleanupService> logger;
        private readonly IServiceProvider serviceProvider;
        private readonly TimeSpan interval;
        private CancellationTokenSource source;

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

            this.logger = logger ?? throw
                new ArgumentNullException(nameof(logger));

            this.serviceProvider = serviceProvider ?? throw
                new ArgumentNullException(nameof(serviceProvider));

            this.interval = TimeSpan.FromSeconds(options.TokenCleanupInterval);
        }

        public void Start()
        {
            Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (this.source != null)
            {
                throw new InvalidOperationException(
                    "Already started. Call Stop first.");
            }

            this.logger.LogDebug("Starting token cleanup");

            this.source = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternal(this.source.Token));
        }

        public void Stop()
        {
            if (this.source == null)
            {
                throw new InvalidOperationException(
                        "Not started. Call Start first.");
            }

            this.logger.LogDebug("Stopping token cleanup");

            this.source.Cancel();
            this.source = null;
        }

        private async Task StartInternal(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    await Task.Delay(this.interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    this.logger.LogDebug("TaskCanceledException. Exiting.");
                    break;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        "Task.Delay exception: {0}. Exiting.",
                        ex.Message);
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    this.logger.LogDebug("CancellationRequested. Exiting.");
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
                this.logger.LogError(
                    "Exception clearing tokens: {exception}",
                    ex.Message);
            }
        }

        public void ClearTokens()
        {
            this.logger.LogTrace("Querying for tokens to clear");

            using (IServiceScope serviceScope = this.serviceProvider
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (IPersistedGrantDbContext context = serviceScope
                    .ServiceProvider.GetService<IPersistedGrantDbContext>())
                {
                    PersistedGrant[] expired = context.PersistedGrants
                        .Where(x => x.Expiration < DateTimeOffset.UtcNow)
                        .ToArray();

                    this.logger.LogInformation(
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