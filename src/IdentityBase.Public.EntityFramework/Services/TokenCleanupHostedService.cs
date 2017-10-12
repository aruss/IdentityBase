// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection
{
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase.Public.EntityFramework.Options;
    using IdentityBase.Public.EntityFramework.Services;
    using Microsoft.Extensions.Hosting;

    internal class TokenCleanupHostedService : IHostedService
    {
        private readonly TokenCleanupService _tokenCleanup;
        private readonly EntityFrameworkOptions _options;

        public TokenCleanupHostedService(
            TokenCleanupService tokenCleanup,
            EntityFrameworkOptions options)
        {
            _tokenCleanup = tokenCleanup;
            _options = options;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_options.EnableTokenCleanup)
            {
                _tokenCleanup.Start(cancellationToken);
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_options.EnableTokenCleanup)
            {
                _tokenCleanup.Stop();
            }
            return Task.CompletedTask;
        }
    }
}