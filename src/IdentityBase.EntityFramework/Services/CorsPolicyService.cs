namespace IdentityBase.EntityFramework.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.EntityFramework.Interfaces;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of ICorsPolicyService that consults the client
    /// configuration in the database for allowed CORS origins.
    /// </summary>
    /// <seealso cref="IdentityServer4.Services.ICorsPolicyService" />
    public class CorsPolicyService : ICorsPolicyService
    {
        private readonly IHttpContextAccessor _context;
        private readonly ILogger<CorsPolicyService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorsPolicyService"/>
        /// class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public CorsPolicyService(
            IHttpContextAccessor context,
            ILogger<CorsPolicyService> logger)
        {
            this._context = context ?? throw
                new ArgumentNullException(nameof(context));

            this._logger = logger;
        }

        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public Task<bool> IsOriginAllowedAsync(string origin)
        {
            // Doing this here and not in the ctor
            // because: https://github.com/aspnet/CORS/issues/105
            IConfigurationDbContext dbContext = this._context.HttpContext
                .RequestServices.GetRequiredService<IConfigurationDbContext>();

            IEnumerable<string> origins = dbContext.Clients
                .SelectMany(x => x.AllowedCorsOrigins.Select(y => y.Origin))
                .ToList();

            IEnumerable<string> distinctOrigins = origins
                .Where(x => x != null)
                .Distinct();

            bool isAllowed = distinctOrigins
                .Contains(origin, StringComparer.OrdinalIgnoreCase);

            this._logger.LogDebug(
                "Origin {origin} is allowed: {originAllowed}",
                origin,
                isAllowed);

            return Task.FromResult(isAllowed);
        }
    }
}