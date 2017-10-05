using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ServiceBase.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;

namespace IdentityBase.Public.IntegrationTests
{
    public static class TestServerBuilder
    {
        public static HttpResponseMessage GetAndAssert(this HttpClient client, string url)
        {
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            return response;
        }

        public static TestServer BuildServer<TStartup>(IConfigurationRoot configuration)
            where TStartup : class
        {
            return BuildServer<TStartup>(configuration, null);
        }

        public static TestServer BuildServer<TStartup>(Action<IServiceCollection> configureServices)
            where TStartup : class
        {
            return BuildServer<TStartup>(null, configureServices);
        }

        public static TestServer BuildServer<TStartup>(IConfigurationRoot configuration = null,
            Action<IServiceCollection> configureServices = null) where TStartup : class
        {
            if (configuration == null)
            {
                configuration = ConfigBuilder.BuildConfig();
            }

            var contentRoot = Path.GetFullPath(Path.Combine(
                Directory.GetCurrentDirectory(), "src/IdentityBase.Public"));
            if (!Directory.Exists(contentRoot))
            {
                contentRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                    "../../../../../src/IdentityBase.Public"));
            }

            var environment = new HostingEnvironment
            {
                EnvironmentName = "Development",
                ApplicationName = "IdentityBase.Public",
                ContentRootPath = contentRoot,
                ContentRootFileProvider = new PhysicalFileProvider(contentRoot)
            };

            var logger = new NullLogger<Startup>();
            var startup = new Startup(configuration, environment, logger); 

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureServices(services =>
                {
                    configureServices?.Invoke(services);
                    services.AddSingleton<IStartup>(startup);
                })
            // WORKARROUND: https://github.com/aspnet/Hosting/issues/1137#issuecomment-323234886
                .UseSetting(WebHostDefaults.ApplicationKey,
                    typeof(Startup).GetTypeInfo().Assembly.FullName);

            return new TestServer(builder);
        }
    }
}
