using Microsoft.AspNetCore.TestHost;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    [Collection("General Tests")]
    public class GeneralTests
    {
        private HttpClient _client;
        private TestServer _server;

        public GeneralTests()
        {
            var config = ConfigBuilder.Default.Build();
            _server = TestServerBuilder.BuildServer<Startup>(config);
            _client = _server.CreateClient();
        }

        [Fact(DisplayName = "Get_LandingPage")]
        public async Task Get_LandingPage()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Default configuration disables all invitation functionality
        /// Listing of all invitastions should not be enabled 
        /// </summary>
        [Fact(DisplayName = "WebAPI_Get_Invitations_Should_Be_Disabled")]
        public async Task WebAPI_Get_Invitations_Should_Be_Disabled()
        {
            // Act
            var response = await _client.GetAsync("/api/invitations");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound,
                "GET /api/invitations should return 404");
        }

        [Fact(DisplayName = "WebAPI_Put_Invitations_Should_Be_Disabled")]
        public async Task WebAPI_Put_Invitations_Should_Be_Disabled()
        {
            // Act
            var response = await _client.PutJsonAsync("/api/invitations"); 
            
            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound,
                "PUT /api/invitations should return 404");
        }

        [Fact(DisplayName = "WebAPI_Delete_Invitations_Should_Be_Disabled")]
        public async Task WebAPI_Delete_Invitations_Should_Be_Disabled()
        {
            // Act
            var response = await _client.DeleteAsync("/api/invitations/5122fa89-cb91-4f07-b179-f4d29ee2a353");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.NotFound,
                "DELETE /api/invitations/5122fa89-cb91-4f07-b179-f4d29ee2a353 should return 404");
        }        
    }
}
