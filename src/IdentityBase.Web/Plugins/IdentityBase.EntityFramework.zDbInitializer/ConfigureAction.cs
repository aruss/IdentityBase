// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.DbInitializer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ExampleDataStoreInitializer initializer = serviceScope
                    .ServiceProvider
                    .GetService<ExampleDataStoreInitializer>();

                if (initializer != null)
                {
                    initializer.InitializeStores();
                }
            }
        }
    }
}
