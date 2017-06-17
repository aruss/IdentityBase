using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    public class RecoverTests
    {
        private HttpClient _client;
        private TestServer _server;

        public RecoverTests()
        {
            var config = ConfigBuilder.Default.Build();
            _server = TestServerBuilder.BuildServer<Startup>(config);
            _client = _server.CreateClient();
        }

        [Theory(DisplayName = "Try_Recover")]

        // try to recover not existent user
        [InlineData("nothere@localhost", true)]

        // try to recover inactive user
        [InlineData("jim@localhost", true)]

        // try to recover with invalid email
        [InlineData("alice_localhost", true)]
        public async Task Try_Recover(string email, bool isError)
        {
            // Call the login page 
            var response = await _client.GetAsync($"/recover?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", email },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() },
                { "ReturnUrl", doc.GetReturnUrl() }
            };

            var response2 = await _client.PostFormAsync(doc.GetFormAction(), form, response);
            var doc2 = await response2.Content.ReadAsHtmlDocumentAsync();

            // TODO: check response
        }
    }
}

