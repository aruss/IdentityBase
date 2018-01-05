namespace IdentityBase.IntegrationTests
{

    using System;
    using System.IO;
    using System.Net.Http;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using ServiceBase.Extensions;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;

    public static class TestServerBuilderExtensions
    {
        public static TestServerBuilder UseContentRoot(
            this TestServerBuilder builder)
        {
            string contentRoot = Path.GetFullPath(Path.Combine(
              Directory.GetCurrentDirectory(), "src/IdentityBase"));

            if (!Directory.Exists(contentRoot))
            {
                contentRoot = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    "../../../../../src/IdentityBase"));
            }

            if (!Directory.Exists(contentRoot))
            {
                throw new DirectoryNotFoundException(contentRoot);
            }

            return builder.UseContentRoot(contentRoot);
        }

        /// <summary>
        /// Creates test server with identity base startup file.
        /// </summary>
        /// <param name="emailServiceMock"></param>
        /// <param name="configBuild"></param>
        /// <returns></returns>
        public static TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null,
            Action<TestConfigurationBuilder> configBuild = null)
        {
            TestServer testServer = null;

            Func<HttpMessageHandler> messageHandlerFactory = () =>
            {
                return testServer.CreateHandler();
            };

            testServer = new TestServerBuilder()
                .UseEnvironment("Test")
                .UseContentRoot()
                .AddStartup((environment) =>
                {
                    var builder = new TestConfigurationBuilder()
                        .UseDefaultConfiguration();

                    if (configBuild != null)
                    {
                        configBuild.Invoke(builder);
                    }

                    var startup = new IdentityBase.Startup(
                        builder.Build(),
                        environment,
                        new NullLoggerFactory(),
                        messageHandlerFactory
                    );

                    if (emailServiceMock != null)
                    {
                        startup.OverrideServices = (services =>
                        {
                            services.Replace(emailServiceMock.Object);
                        });
                    }

                    return startup;
                })
                .Build();

            return testServer;
        }
    }
}