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

            /*System.IO.File.WriteAllText(@"./config/data_clients.json", 
                JsonConvert.SerializeObject(ServiceBase.IdentityServer.Configuration.Clients.Get()));

            System.IO.File.WriteAllText(@"./config/data_resources_api.json",
                JsonConvert.SerializeObject(ServiceBase.IdentityServer.Configuration.Resources.GetApiResources()));

            System.IO.File.WriteAllText(@"./config/data_resources_identity.json",
                JsonConvert.SerializeObject(Resources.GetIdentityResources()));

            var crypto = new DefaultCrypto();
            var config = new ApplicationOptions(); 
            System.IO.File.WriteAllText(@"./config/data_users.json",
                JsonConvert.SerializeObject(UserAccounts.Get(crypto, config)));*/
        }
    }
}
