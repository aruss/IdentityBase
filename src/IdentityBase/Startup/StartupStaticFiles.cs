// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    public static class StartupStaticFiles
    {
        public static void UseStaticFiles(
            this IApplicationBuilder app,
            ApplicationOptions options,
            IHostingEnvironment environment)
        {
            var basePath = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Themes")); 
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new ThemedFileProvider(basePath)
            });
        }
    }
}
