// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.SqlServer
{
    using Microsoft.AspNetCore.Builder;
    using ServiceBase.Plugins;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder app)
        {
            app.WaitForDatabase(); 
        }
    }
}
