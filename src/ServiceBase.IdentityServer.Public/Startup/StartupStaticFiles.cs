using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupStaticFiles
    {
        public static void UseStaticFiles(this IApplicationBuilder app, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(GetStaticFilesPath(config)),
            });
        }

        private static string GetStaticFilesPath(IConfigurationRoot config)
        {
            var staticFilesPath = config["App:ThemePath"];
            if (!String.IsNullOrWhiteSpace(staticFilesPath))
            {
                staticFilesPath = Path.IsPathRooted(staticFilesPath)
                    ? Path.Combine(staticFilesPath, "Public")
                    : Path.Combine(Directory.GetCurrentDirectory(), staticFilesPath, "Public");

                if (Directory.Exists(staticFilesPath))
                {
                    return staticFilesPath; 
                }
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "Themes", "Default", "Public");
        }
    }
}
