// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase;
    using ServiceBase.DependencyInjection;
    using ServiceBase.Notification.Email;

    public static partial class IServiceCollectionExtensions
    {
        public static void AddDefaultEmailService(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (!services.IsAdded<ITokenizer>())
            {
                services.AddScoped<ITokenizer, DefaultTokenizer>();
            }

            services.AddScoped<IEmailService, DefaultEmailService>();
            services.AddFactory<DefaultEmailServiceOptions, DefaultEmailServiceOptionsFactory>();
        }
    }
}
