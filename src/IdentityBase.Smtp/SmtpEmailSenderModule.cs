// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Smtp
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Email;
    using ServiceBase.Notification.Smtp;

    public class SmtpEmailSenderModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDefaultEmailService(configuration); 

            services.AddScoped<IEmailSender, SmtpEmailSender>();
            
            services.AddSingleton(configuration
                .GetSection("Email:Smtp").Get<SmtpOptions>());
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
