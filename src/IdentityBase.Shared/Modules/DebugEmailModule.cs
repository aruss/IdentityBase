// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Email;

    public class DebugEmailModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDefaultEmailService(configuration); 
            services.AddScoped<IEmailSender, DebugEmailSender>();
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}