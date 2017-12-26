// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Sms;

    public static class StartupSmsSender
    {
        public static void ValidateSmsServices(
            this IServiceCollection services,
            ILogger logger)
        {
            if (!services.IsAdded<ISmsService>())
            {
                throw new Exception("ISmsService not registered.");
            }
        }
    }
}
