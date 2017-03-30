using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ServiceBase.IdentityServer.Public
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:5000/")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                // .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
