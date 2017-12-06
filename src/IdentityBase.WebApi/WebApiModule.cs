// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi
{
    using IdentityBase.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Modules;

    public class WebApiModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {

        }

        public void Configure(IApplicationBuilder appBuilder)
        {
            IConfiguration config = appBuilder.ApplicationServices
                .GetRequiredService<IConfiguration>();

            IHostingEnvironment environment = appBuilder.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            ILogger<Startup> logger = appBuilder.ApplicationServices
                .GetRequiredService<ILogger<Startup>>();

            Startup startup = new Startup(config, environment, logger); 

            appBuilder.MapStartup(
                "/api",
                environment,
                config,
                (services) =>
                {
                    startup.ConfigureServices(services);
                },
                (app) =>
                {
                    startup.Configure(app); 
                });
        }        
    }
}
