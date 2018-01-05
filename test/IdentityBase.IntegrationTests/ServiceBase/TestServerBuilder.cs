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

    /// <summary>
    /// <see cref="TestServer"/> factory.
    /// </summary>
    public class TestServerBuilder
    {
        private string contentRoot;
        private IConfiguration configuration;
        private string environment;
        private Action<IServiceCollection> configureServices;
        private Func<IHostingEnvironment, IStartup> createStartup;

        /// <summary>
        /// Specify the environment to be used by the web host.
        /// </summary>
        /// <param name="environment">The environment to host the
        /// application in.</param>
        /// <returns>Instance of <see cref="TestServerBuilder"/></returns>
        public TestServerBuilder UseEnvironment(string environment)
        {
            this.environment = environment;
            return this;
        }

        /// <summary>
        /// Specify the content root directory to be used by the web host.
        /// </summary>
        /// <param name="contentRoot">Path to root directory of the
        /// application.</param>
        /// <returns>Instance of <see cref="TestServerBuilder"/></returns>
        public TestServerBuilder UseContentRoot(string contentRoot)
        {
            this.contentRoot = contentRoot;
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host
        /// or web application. This may be called multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the
        /// <see cref="IServiceCollection"/>.</param>
        /// <returns>Instance of <see cref="TestServerBuilder"/></returns>
        public TestServerBuilder AddServices(
           Action<IServiceCollection> configureServices)
        {
            this.configureServices = configureServices;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createStartup"></param>
        /// <returns>Instance of <see cref="TestServerBuilder"/></returns>
        public TestServerBuilder AddStartup(
           Func<IHostingEnvironment, IStartup> createStartup)
        {
            this.createStartup = createStartup;
            return this;
        }

        /// <summary>
        /// Builds the required services and an <see cref="TestServer"/>
        /// which hosts a web application.
        /// </summary>
        /// <returns>Instance of <see cref="TestServer"/>.</returns>
        public TestServer Build()
        {
            // this.Validate();

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
    }
}
