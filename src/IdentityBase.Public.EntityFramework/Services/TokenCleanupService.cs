// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.EntityFramework.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.Public.EntityFramework.DbContexts;
    using IdentityBase.Public.EntityFramework.Options;
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
            if (options == null) throw
                    new ArgumentNullException(nameof(options));

            if (options.TokenCleanupInterval < 1) throw
                    new ArgumentException(
                        "interval must be more than 1 second");

            _logger = logger ?? throw
                new ArgumentNullException(nameof(logger));

            _serviceProvider = serviceProvider ?? throw
                new ArgumentNullException(nameof(serviceProvider));

            _interval = TimeSpan.FromSeconds(options.TokenCleanupInterval);
        }

        public void Start()
        {
            Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (_source != null) throw
                    new InvalidOperationException(
                        "Already started. Call Stop first.");

            _logger.LogDebug("Starting token cleanup");

            _source = CancellationTokenSource
                .CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternal(_source.Token));
        }

        public void Stop()
        {
            if (_source == null) throw
                    new InvalidOperationException(
                        "Not started. Call Start first.");

            _logger.LogDebug("Stopping token cleanup");

            _source.Cancel();
            _source = null;
        }

        private async Task StartInternal(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("CancellationRequested. Exiting.");
                    break;
                }

                try
                {
                    await Task.Delay(_interval, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    _logger.LogDebug("TaskCanceledException. Exiting.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        "Task.Delay exception: {0}. Exiting.",
                        ex.Message);
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("CancellationRequested. Exiting.");
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
                _logger.LogError(
                    "Exception clearing tokens: {exception}",
                    ex.Message);
            }
        }

        public void ClearTokens()
        {
            _logger.LogTrace("Querying for tokens to clear");

            using (var serviceScope = _serviceProvider
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider
                    .GetService<PersistedGrantDbContext>())
                {
                    var expired = context.PersistedGrants
                        .Where(x => x.Expiration < DateTimeOffset.UtcNow)
                        .ToArray();

                    _logger.LogInformation(
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