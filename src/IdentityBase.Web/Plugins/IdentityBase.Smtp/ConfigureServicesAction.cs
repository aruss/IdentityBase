// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Smtp
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Notification.Email;
    using ServiceBase.Notification.Smtp;
    using ServiceBase.Plugins;

    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services
                .BuildServiceProvider();

            IConfiguration configuration = serviceProvider
                .GetService<IConfiguration>();

            services.AddDefaultEmailService(configuration);

            services.AddScoped<IEmailSender, SmtpEmailSender>();

            services.AddSingleton(configuration
                .GetSection("Email:Smtp").Get<SmtpOptions>());
        }
    }
}
