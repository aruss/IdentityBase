// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.GoogleRecaptcha
{
    using System;
    using IdentityBase.Forms;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;
    
    //[DependsOnPlugin("Host")]
    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services
                .BuildServiceProvider();

            IConfiguration configuration = serviceProvider
                .GetService<IConfiguration>();

            services.AddSingleton(configuration
                .GetSection("GoogleRecaptcha").Get<GoogleRecaptchaOptions>());

            services.AddScoped<ILoginCreateViewModelAction,
                GoogleRecaptchaCreateViewModelAction>();

            services.AddScoped<IRecoverCreateViewModelAction,
                GoogleRecaptchaCreateViewModelAction>();

            services.AddScoped<ILoginBindInputModelAction,
                GoogleRecaptchaBindInputModelAction>();

            services.AddScoped<IRecoverBindInputModelAction,
                GoogleRecaptchaBindInputModelAction>();

            services.AddHttpClient("GoogleRecaptchaClient", client =>
            {
                 client.BaseAddress = new Uri("https://www.google.com");
            }); 
        }
    }
}
