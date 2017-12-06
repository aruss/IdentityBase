// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Events;

    public static class StartupEvents
    {
        public static void ValidateEventServices(
            this IServiceCollection services,
            ILogger logger)
        {
            if (!services.IsAdded<IEventService>())
            {
                throw new Exception("IEventService not registered.");
            }
        }
    }
}
