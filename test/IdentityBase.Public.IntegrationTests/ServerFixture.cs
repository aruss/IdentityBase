using IdentityBase.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace IdentityBase.Public.IntegrationTests
{
    public class ServerFixture : IDisposable
    {
        public TestServer Server { get; private set; }
        public HttpClient Client { get; private set; }

        public ServerFixture()
        {
            this.Server = ServerHelper.CreateServer((services) =>
            {
                services.AddSingleton(new ApplicationOptions());
            });

            this.Client = this.Server.CreateClient();
        }

        public void Dispose()
        {
        }
    }
}