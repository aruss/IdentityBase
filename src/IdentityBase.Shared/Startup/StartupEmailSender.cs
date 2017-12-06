// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
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
                throw new Exception("IEmailService not registered.");
            }
        }
    }
}
