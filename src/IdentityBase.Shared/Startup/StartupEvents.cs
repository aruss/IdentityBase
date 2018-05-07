// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using Microsoft.Extensions.Configuration;
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
                logger.LogWarning(
                    $"IEventService not registered. Registering \"{nameof(DefaultEventService)}\" and \"{ nameof(DefaultEventSink)}\"");

                IServiceProvider serviceProvider = services
                    .BuildServiceProvider();

                IConfiguration config = serviceProvider
                    .GetService<IConfiguration>();

                services.AddSingleton(config.GetSection("Events")
                    .Get<EventOptions>() ?? new EventOptions());

                services.AddScoped<IEventService, DefaultEventService>();
                services.AddScoped<IEventSink, DefaultEventSink>();

                // throw new Exception("IEventService not registered.");
            }
        }
    }
}
