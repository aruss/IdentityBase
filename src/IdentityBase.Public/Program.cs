using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBase.Configuration;
using ServiceBase.Extensions;
using System;
using System.IO;

namespace IdentityBase.Public
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunIdentityBase<Startup>(Directory.GetCurrentDirectory(), args);
        }

        public static void RunIdentityBase<TStartup>(string contentRoot, string[] args) where TStartup : Startup
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = ConfigurationSetup.Configure(contentRoot, environment, (confBuilder) =>
            {
                if ("Development".Equals(environment, StringComparison.OrdinalIgnoreCase))
                {
                    confBuilder.AddUserSecrets<TStartup>();
                }

                confBuilder.AddCommandLine(args);
            });

            var configHost = configuration.GetSection("Host");
            var configLogging = configuration.GetSection("Logging");

            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(configHost["Urls"])
                .UseContentRoot(contentRoot)
                .ConfigureLogging(f => f.AddConsole(configLogging))
                .UseStartup<TStartup>();

            if (configHost["UseIISIntegration"].ToBoolean())
            {
                hostBuilder = hostBuilder.UseIISIntegration();
            }

            hostBuilder.Build().Run();
        }
    }
}
