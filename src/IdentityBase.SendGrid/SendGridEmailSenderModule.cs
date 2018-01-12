// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.SendGrid
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Email;
    using ServiceBase.Notification.SendGrid;

    public class SendGridEmailSenderModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDefaultEmailService(configuration);

            services.AddScoped<IEmailSender, SendGridEmailSender>();

            services.AddSingleton(configuration
                .GetSection("Email:SendGrid").Get<SendGridOptions>());
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
