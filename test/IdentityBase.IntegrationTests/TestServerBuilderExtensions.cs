

namespace IdentityBase.IntegrationTests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityBase;
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
        /// <param name="messageHandler"></param>
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

                    return new IdentityBase.Startup(
                        builder.Build(),
                        environment,
                        new NullLoggerFactory(),
                        messageHandlerFactory
                    );
                })
                .Build();


            return testServer;
        }
    }
}

namespace System.Net.Http
{
    using System.Threading;
    using System.Threading.Tasks;

    public class TestHttpMessageHandler : HttpMessageHandler
    {
        private HttpMessageHandler _messageHandler;
        private Func<HttpMessageHandler> _createHandler;

        public TestHttpMessageHandler(Func<HttpMessageHandler> createHandler)
        {
            this._createHandler = createHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
