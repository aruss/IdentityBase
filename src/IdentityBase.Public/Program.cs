// [assembly: UserSecretsId("IdentityBase.Public")]

namespace IdentityBase.Public
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ServiceBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = ConfigUtils.LoadConfiguration<Startup>(
                path: "./AppData/config.json",
                basePath: Directory.GetCurrentDirectory(),
                args: args
            );

            IConfigurationSection configHost = config.GetSection("Host");

            IWebHostBuilder hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseUrls(configHost.GetValue<string>("Urls"))
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(hostingContext.Configuration);
                })
                .UseStartup<Startup>();

            if (configHost.GetValue<bool>("UseIISIntegration"))
            {
                Console.WriteLine("Enabling IIS Integration");
                hostBuilder = hostBuilder.UseIISIntegration();
            }
           
            hostBuilder
                .Build()
                .Run();
        }
    }
}