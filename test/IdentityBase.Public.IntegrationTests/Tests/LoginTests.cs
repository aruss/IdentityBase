using AngleSharp.Parser.Html;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests
{
    // http://stackoverflow.com/questions/30557521/how-to-access-httpcontext-inside-a-unit-test-in-asp-net-5-mvc-6

    [Collection("Login")]
    public class LoginTests : IClassFixture<ServerFixture>
    {
        private string _returnUrl = "%2Fconnect%2Fauthorize%2Flogin%3Fclient_id%3Dmvc%26redirect_uri%3Dhttp%253A%252F%252Flocalhost%253A3308%252Fsignin-oidc%26response_type%3Dcode%2520id_token%26scope%3Dopenid%2520profile%2520api1%26response_mode%3Dform_post%26nonce%3D636170876883483776.ZGUwYWY2NDctNDJlNy00MTVmLTkwZTYtZjVjMTQ4ZWVlMzAwMWM2OWNhODQtYzZjOS00ZDljLTk3NTktYWE1ZWExMDEwYzk2%26state%3DCfDJ8McEKbBuVCdHkFjjPyy6vSPN5QZvt6xKTHnnKEyNzXwN1YpWo0Mslqn-wBoHhp9vMSjqo3GQGU7emMMhZlgu0BK3G03m2uqLc5vrYBz06tcWr8S4f9oKl2u1S0cAiJEOw13GnuF-EJ0E3by0nUJ3m1MhhnovobqqTEpKMldmLGpaUxPS4YGxSQVgzDzo3XsyHB4KvWlsdnb3InqNoPKnTQ4ljgDOAeKTAMj39Jz1SMauTcfOXHDyCnJdLt7I0v0up1oY5Az9b7xjzk0oBq5P7lADyq88YTEG0EALJG8SgjYi-Ch-0jd26w74LJ5UyQNScc1ZS4n9dMKUHXvuuIWllzNK86la5X-ydnsNZo2a1HsHyPT4NHe6EG2LdVkh6Y-2-A";
        private HttpClient _client;
        private TestServer _server;

        public LoginTests(ServerFixture fixture)
        {
            _server = fixture.Server;
            _client = fixture.Client;
        }

        [Fact]
        public async Task LoginWithValidUserNotRemember()
        {
            // Call login page
            var getResponse = await _client.GetAsync("/login?returnUrl=" + _returnUrl);
            getResponse.EnsureSuccessStatusCode();

            // Create post body for login request
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(getResponseContent));
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","alice@localhost"},
                {"Password", "alice@localhost"},
                {"RememberLogin", "false"},
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };
            var postRequest = getResponse.CreatePostRequest("/login", formPostBodyData);

            var postResponse = await _client.SendAsync(postRequest);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");

            await Logout(postResponse);
        }

        private async Task Logout(HttpResponseMessage responseFrom)
        {
            var getRequest = responseFrom.CreateGetRequest("/logout");
            var getResponse = await _client.SendAsync(getRequest);
            var content = await getResponse.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(content));

            var formPostBodyData = new Dictionary<string, string>
            {
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var postRequest = getResponse.CreatePostRequest("/logout", formPostBodyData);
            var postResponse = await _client.SendAsync(postRequest);
        }

        [Fact]
        public async Task LoginWithValidUserRemember()
        {
            // Call login page
            var getResponse = await _client.GetAsync("/login?returnUrl=" + _returnUrl);
            getResponse.EnsureSuccessStatusCode();

            // Create post body for login request
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(getResponseContent));
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","alice@localhost"},
                {"Password", "alice@localhost"},
                {"RememberLogin", "true"},
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };
            var postRequest = getResponse.CreatePostRequest("/login", formPostBodyData);

            var postResponse = await _client.SendAsync(postRequest);

            postResponse.StatusCode.Should().Be(HttpStatusCode.Found);
            postResponse.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
        }

        [Fact]
        public async Task LoginWithWrongPassword()
        {
            // Call login page
            var getResponse = await _client.GetAsync("/login?returnUrl=" + _returnUrl);
            getResponse.EnsureSuccessStatusCode();

            // Create post body for login request
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(getResponseContent));
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","alice@localhost"},
                {"Password", "wrongpassword"},
                {"RememberLogin", "false"},
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = getResponse.CreatePostRequest("/login", formPostBodyData);
            var postResponse = await _client.SendAsync(postRequest);
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task LoginWithWrongUser()
        {
            // Call login page
            var getResponse = await _client.GetAsync("/login?returnUrl=" + _returnUrl);
            getResponse.EnsureSuccessStatusCode();

            // Create post body for login request
            var getResponseContent = await getResponse.Content.ReadAsStringAsync();
            var doc = (new HtmlParser().Parse(getResponseContent));
            var formPostBodyData = new Dictionary<string, string>
            {
                {"Email","notthere@localhost"},
                {"Password", "wrongpassword"},
                {"RememberLogin", "false"},
                {"__RequestVerificationToken", doc.GetAntiForgeryToken() },
                {"ReturnUrl", doc.GetReturnUrl()}
            };

            var postRequest = getResponse.CreatePostRequest("/login", formPostBodyData);
            var postResponse = await _client.SendAsync(postRequest);
            postResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // TODO: check for error message
        }
    }
}