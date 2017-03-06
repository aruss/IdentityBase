// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using ServiceBase.IdentityServer.Public.EF.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EF.Services
{
    public class CorsPolicyService : ICorsPolicyService
    {
        private readonly IConfigurationDbContext _context;
        private readonly ILogger<CorsPolicyService> _logger;

        public CorsPolicyService(IConfigurationDbContext context, ILogger<CorsPolicyService> logger)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            _context = context;
            _logger = logger;
        }

        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            var origins = _context.Clients.SelectMany(x => x.AllowedCorsOrigins.Select(y => y.Origin)).ToList();

            var distinctOrigins = origins.Where(x => x != null).Distinct();

            var isAllowed = distinctOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug("Origin {origin} is allowed: {originAllowed}", origin, isAllowed);

            return Task.FromResult(isAllowed);
        }
    }
}