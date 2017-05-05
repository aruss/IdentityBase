using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBase.Configuration;
using System;
using System.IO;


namespace ServiceBase.IdentityServer.Public
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }

        public static void Main(string[] args)
        {
            var contentRoot = Directory.GetCurrentDirectory();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var configuration = ConfigurationSetup.Configure(contentRoot, environment, (confBuilder) =>
            {
                if ("Development".Equals(environment, StringComparison.OrdinalIgnoreCase))
                {
                    confBuilder.AddUserSecrets<Startup>();
                }

                confBuilder.AddCommandLine(args);
            });

            var hostConfig = configuration.GetSection("Host");

            var hostBuilder = new WebHostBuilder()
                .UseUrls(hostConfig["Urls"])
                .UseContentRoot(Directory.GetCurrentDirectory())
                //.UseConfiguration(configuration.GetSection("Kestrel"))
                .UseKestrel()
                .ConfigureLogging(f => f.AddConsole(configuration.GetSection("Logging")))
                .UseStartup<Startup>();

            if (!String.IsNullOrWhiteSpace(hostConfig["UseIISIntegration"]))
            {
                hostBuilder = hostBuilder.UseIISIntegration(); 
            }
            
            hostBuilder.Build().Run();
        }
    }
}
