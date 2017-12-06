namespace Microsoft.Extensions.DependencyInjection
{
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Services;
    using Microsoft.Extensions.Hosting;

    internal class TokenCleanupHostedService : IHostedService
    {
        private readonly TokenCleanupService _tokenCleanup;
        private readonly EntityFrameworkOptions _options;

        public TokenCleanupHostedService(
            TokenCleanupService tokenCleanup,
            EntityFrameworkOptions options)
        {
            this._tokenCleanup = tokenCleanup;
            this._options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (this._options.EnableTokenCleanup)
            {
                this._tokenCleanup.Start(cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (this._options.EnableTokenCleanup)
            {
                this._tokenCleanup.Stop();
            }

            return Task.CompletedTask;
        }
    }
}