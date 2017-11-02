namespace ServiceBase.Tests
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;

    public class TestServerBuilder
    {
        private string contentRoot;
        private IConfiguration configuration;
        private string environment;
        private Action<IServiceCollection> configureServices;
        private Func<IHostingEnvironment, IStartup> createStartup;

        public TestServerBuilder UseEnvironment(string environment)
        {
            this.environment = environment;
            return this;
        }

        public TestServerBuilder UseContentRoot(string contentRoot)
        {
            this.contentRoot = contentRoot;
            return this;
        }

        public TestServerBuilder AddServices(
           Action<IServiceCollection> configureServices)
        {
            this.configureServices = configureServices;
            return this;
        }

        public TestServerBuilder AddStartup(
           Func<IHostingEnvironment, IStartup> createStartup)
        {
            this.createStartup = createStartup;
            return this;
        }

        public TestServer Build()
        {
            this.Validate();

            IHostingEnvironment environment = new HostingEnvironment
            {
                EnvironmentName = this.environment,
                // ApplicationName = "TestApp",
                ContentRootPath = this.contentRoot,
                ContentRootFileProvider =
                    new PhysicalFileProvider(this.contentRoot)
            };

            IStartup startup = this.createStartup.Invoke(environment);

            string appName = startup.GetType().GetTypeInfo().Assembly.FullName;

            IWebHostBuilder builder = new WebHostBuilder()
                .UseContentRoot(this.contentRoot)
                .UseEnvironment(this.environment)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);

                    this.configureServices?.Invoke(services);
                })
                // WORKARROUND: https://github.com/aspnet/Hosting/issues/1137#issuecomment-323234886
                .UseSetting(WebHostDefaults.ApplicationKey, appName);

            return new TestServer(builder);
        }

        private void Validate()
        {

        }
    }
}
