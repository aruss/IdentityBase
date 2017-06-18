using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    [Collection("Login Tests")]
    public class Login2Tests
    {
        private HttpClient _client;
        private TestServer _server;

        public Login2Tests()
        {
            var config = ConfigBuilder
                .Default
                .RemoveAuthFacebook() // left only one identity provider
                .Alter("App:EnableLocalLogin", "false") // disable local login
                .Build();

            _server = TestServerBuilder.BuildServer<Startup>(config);
            _client = _server.CreateClient();
        }

        // This will force user directly to thirdparty auth server
        [Fact(DisplayName = "Get_LoginPage_With_IsExternalLoginOnly_Option")]
        public async Task Get_LoginPage_With_IsExternalLoginOnly_Option()
        {
            // Act
            var response = await _client.GetAsync($"/login?returnUrl={Constants.ReturnUrl}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location
                .ToString().Should().StartWith("https://accounts.google.com/o/oauth2");
        }
    }

    [Collection("Login Tests")]
    public class LoginTests
    {
        private HttpClient _client;
        private TestServer _server;

        public LoginTests()
        {
            var config = ConfigBuilder.Default.Build();
            _server = TestServerBuilder.BuildServer<Startup>(config);
            _client = _server.CreateClient();
        }

        [Fact(DisplayName = "Get_LoginPage_Without_Args_Should_Redirect_To_LandingPage")]
        public async Task Get_LoginPage_Without_Args_Should_Redirect_To_LandingPage()
        {
            // Act
            var response = await _client.GetAsync("/login");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.ToString().Should().Equals("/");
        }

        /// <summary>
        /// Anti forgery token protection ...
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Post_LoginPage_With_IsExternalLoginOnly_Option_Should_Be_Disabled")]
        public async Task Post_LoginPage_With_IsExternalLoginOnly_Option_Should_Be_Disabled()
        {
            // Act
            var response = await _client.PostFormAsync("/login");

            // Assert
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest,
                "POST /login should return 400");
        }

        [Theory(DisplayName = "Try_Login_With_Local_Account")]

        // Valid user do not remember login
        [InlineData("alice@localhost", "alice@localhost", false, false)]

        // Valid user remember login
        [InlineData("alice@localhost", "alice@localhost", true, false)]

        // Valid user with wrong password, should get an error
        [InlineData("alice@localhost", "test", false, true)]

        // User does not exists, should get an error
        [InlineData("notexists@localhost", "test", false, true)]

        // Inactive user account , should get an error
        [InlineData("jim@localhost", "jim@localhost", false, true)]

        // Not verified user account, should get an error
        [InlineData("paul@localhost", "paul@localhost", false, true)]

        // Only external, has no local account, should receive a hint that he should use his facebook account
        [InlineData("bill@localhost", "doesnothaveone", false, true)]

        // Missing password
        [InlineData("alice@localhost", "", false, true)]

        // Missing email
        [InlineData("", "password", false, true)]

        // Wrong mail 
        [InlineData("thats_not_a_mail", "foobar", false, true)]
        public async Task Try_Login_With_Local_Account(
            string email,
            string password,
            bool rememberMe,
            bool isError)
        {
            // Call the login page 
            var response = await _client.GetAsync($"/login?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password},
                { "RememberLogin", rememberMe ? "true" : "false" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var response2 = await _client.PostFormAsync(doc.GetFormAction(), form, response);

            var statusCode = isError ? HttpStatusCode.OK : HttpStatusCode.Found; 
            if (statusCode == HttpStatusCode.Found)
            {
                // After successfull login user should be redirect to IdentityServer4 authorize endpoint
                response2.StatusCode.Should().Be(HttpStatusCode.Found);
                response2.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
            }
            else
            {
                response2.StatusCode.Should().Be(statusCode);
                
                // Check for error 
                if (isError)
                {
                    var doc2 = await response2.Content.ReadAsHtmlDocumentAsync();
                    var elm = doc2.QuerySelector(".alert.alert-danger");

                    // TODO: check the error message 
                    // elm.TextContent.Contains()
                }
            }
        }

        [Fact(DisplayName = "Try_Login_With_Local_Account_Manipulate_ReturnUri")]
        public async Task Try_Login_With_Local_Account_Manipulate_ReturnUri()
        {
            // Call the login page 
            var response = await _client.GetAsync($"/login?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", "alice@localhost" },
                { "Password", "alice@localhost"},
                { "RememberLogin", "false" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() },
                { "ReturnUrl", "http%3A%2F%2Fmalicous.com" }
            };

            var response2 = await _client.PostFormAsync(doc.GetFormAction(), form, response);

            // Should redirect to startpage, end of journey 
            response2.StatusCode.Should().Be(HttpStatusCode.Found);
            response2.Headers.Location.ToString().Should().Equals("/");
        }
    }
}

