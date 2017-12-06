// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Twilio
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Sms;
    using ServiceBase.Notification.Twilio;

    public class TwilioSmsSenderModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISmsService, DefaultSmsService>();

            services.AddSingleton(configuration
                .GetSection("Sms").Get<DefaultSmsServiceOptions>());

            services.AddScoped<ISmsSender, TwilioSmsSender>();

            services.AddSingleton(configuration
                .GetSection("Sms:Twilio").Get<TwilioOptions>());
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
