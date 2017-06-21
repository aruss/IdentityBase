using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using ServiceBase.Configuration;
using ServiceBase.Extensions;

namespace IdentityBase.Public
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RunIdentityBase<Startup>(Directory.GetCurrentDirectory(), args);
        }

        public static Serilog.Core.Logger Logger; 

        public static void RunIdentityBase<TStartup>(
            string contentRoot,
            string[] args) where TStartup : Startup
        {
            var environment = Environment
                .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = ConfigurationSetup
                .Configure(contentRoot, environment, (confBuilder) =>
            {
                if ("Development".Equals(environment, StringComparison.OrdinalIgnoreCase))
                {
                    confBuilder.AddUserSecrets<TStartup>();
                }

                confBuilder.AddCommandLine(args);
            });

            var logger = new LoggerConfiguration()
               .ReadFrom.ConfigurationSection(configuration.GetSection("Serilog"))
               .CreateLogger();
            Logger = logger; 

            logger.Information("Application Starting");

            var configHost = configuration.GetSection("Host");
            
            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(configHost["Urls"])
                .UseContentRoot(contentRoot)
                .ConfigureLogging(f => f.AddSerilog(logger))
                .UseStartup<TStartup>();

            if (configHost["UseIISIntegration"].ToBoolean())
            {
                logger.Information("Enabling IIS Integration");
                hostBuilder = hostBuilder.UseIISIntegration();
            }

          
            hostBuilder.Build().Run();
        }
    }
}
