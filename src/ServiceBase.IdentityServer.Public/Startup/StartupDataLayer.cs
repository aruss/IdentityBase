using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.IdentityServer.Public.EntityFramework;
using System;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupDataLayer
    {
        public static void AddDataLayer(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            if (String.IsNullOrWhiteSpace(config["EntityFramework"]))
            {
                services.AddEntityFrameworkStores(config.GetSection("EntityFramework"));
            }
            else
            {
                throw new Exception("Store configuration not present");
            }
        }
    }
}
