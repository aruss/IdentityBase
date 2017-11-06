namespace IdentityBase.Public.IntegrationTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using IdentityBase.Public;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;

    public static class TestServerBuilderExtensions
    {
        public static TestServerBuilder UseContentRoot(
            this TestServerBuilder builder)
        {
            string contentRoot = Path.GetFullPath(Path.Combine(
              Directory.GetCurrentDirectory(), "src/IdentityBase.Public"));

            if (!Directory.Exists(contentRoot))
            {
                contentRoot = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    "../../../../../src/IdentityBase.Public"));
            }

            if (!Directory.Exists(contentRoot))
            {
                throw new DirectoryNotFoundException(contentRoot);
            }

            return builder.UseContentRoot(contentRoot);
        }
        
        public static TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null,
            Action<TestConfigurationBuilder> configBuild = null,
            HttpMessageHandler messageHandler = null)
        {
            return new TestServerBuilder()
                .UseEnvironment("Test")
                .UseContentRoot()
                .AddServices(services =>
                {
                    if (emailServiceMock != null)
                    {
                        services.AddSingleton(emailServiceMock.Object);
                    }
                })
                .AddStartup((environment) =>
                {
                    var builder = new TestConfigurationBuilder()
                        .UseDefaultConfiguration();
                    
                    if (emailServiceMock != null)
                    {
                        builder.RemoveDebugEmailModule();
                    }

                    if (configBuild != null)
                    {
                        configBuild.Invoke(builder);
                    }

                    return new Startup(
                        builder.Build(),
                        environment,
                        new NullLogger<Startup>(),
                        messageHandler
                    );
                })
                .Build();
        }
    }
}
