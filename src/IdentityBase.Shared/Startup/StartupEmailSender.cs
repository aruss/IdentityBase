// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public static class StartupEmailSender
    {
        public static void ValidateEmailSenderServices(
            this IServiceCollection services,
            ILogger logger)
        {
            if (!services.IsAdded<IEmailService>())
            {
                logger.LogWarning(
                    $"IEmailService not registered. Registering \"{nameof(DebugEmailSender)}\"");

                IServiceProvider serviceProvider = services
                    .BuildServiceProvider();

                IConfiguration config = serviceProvider
                    .GetService<IConfiguration>();

                services.AddDefaultEmailService(config);
                services.AddScoped<IEmailSender, DebugEmailSender>();
            }            
        }
    }
}
