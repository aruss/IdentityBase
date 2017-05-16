using Microsoft.AspNetCore.TestHost;
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
            this.Server = ServerHelper.CreateServer();
            this.Client = this.Server.CreateClient();
        }

        public void Dispose()
        {
        }
    }
}