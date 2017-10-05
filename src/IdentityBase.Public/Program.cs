// [assembly: UserSecretsId("IdentityBase.Public")]

namespace IdentityBase.Public
{
    using System;
    using System.IO;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            IConfiguration config = Program.LoadConfiguration(args);
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

        public static IConfiguration LoadConfiguration(string[] args)
        {
            bool isDevelopment = Environment
                .GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                .Equals("Development", StringComparison.OrdinalIgnoreCase);

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("./AppData/config.json",
                                optional: false,
                                reloadOnChange: false);

            if (isDevelopment)
            {
                configBuilder.AddUserSecrets<Startup>();
            }

            IConfiguration config = configBuilder
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            return config;
        }
    }
}