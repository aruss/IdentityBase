using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ServiceBase.IdentityServer.Public.IntegrationTests
{
    [Collection("Landing Page")]
    public class GeneralTests
    {
        private HttpClient _client;
        private TestServer _server;

        public GeneralTests()
        {
            _server = ServerHelper.CreateServer();
            _client = _server.CreateClient();
        }

        [Fact]
        public async Task GetIndex()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
        }
    }
}