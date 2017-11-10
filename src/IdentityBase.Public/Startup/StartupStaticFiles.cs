// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public
{
    using System;
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.FileProviders;
    using ServiceBase.Extensions;

    public static class StartupStaticFiles
    {
        public static void UseStaticFiles(
            this IApplicationBuilder app,
            ApplicationOptions options,
            IHostingEnvironment environment)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    GetStaticFilesPath(
                        options.ThemePath,
                        environment.ContentRootPath
                    )
                ),
            });
        }

        private static string GetStaticFilesPath(
            string themePath,
            string contentRootPath)
        {
            if (String.IsNullOrWhiteSpace(themePath))
            {
                throw new ArgumentNullException(nameof(themePath)); 
            }
            
            themePath = Path.GetFullPath(
                Path.IsPathRooted(themePath) ?
                    Path.Combine(themePath.RemoveTrailingSlash(), "Public") :
                    Path.Combine(
                        contentRootPath.RemoveTrailingSlash(),
                        themePath.RemoveTrailingSlash(),
                        "Public"
                    )
                );
            
            return themePath;
        }
    }
}
