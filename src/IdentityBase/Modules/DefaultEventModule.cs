// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Events;
    using ServiceBase.Modules;

    public class DefaultEventModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton(configuration.GetSection("Events")
                .Get<EventOptions>() ?? new EventOptions());

            services.AddScoped<IEventService, DefaultEventService>();
            services.AddScoped<IEventSink, DefaultEventSink>();
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
