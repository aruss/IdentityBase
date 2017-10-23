namespace Microsoft.Extensions.DependencyInjection
{
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.Public.EntityFramework.Options;
    using IdentityBase.Public.EntityFramework.Services;
    using Microsoft.Extensions.Hosting;

    internal class TokenCleanupHostedService : IHostedService
    {
        private readonly TokenCleanupService tokenCleanup;
        private readonly EntityFrameworkOptions options;

        public TokenCleanupHostedService(
            TokenCleanupService tokenCleanup,
            EntityFrameworkOptions options)
        {
            this.tokenCleanup = tokenCleanup;
            this.options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (this.options.EnableTokenCleanup)
            {
                this.tokenCleanup.Start(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.options.EnableTokenCleanup)
            {
                this.tokenCleanup.Stop();
            }
            return Task.CompletedTask;
        }
    }
}