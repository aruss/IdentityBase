using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Extensions;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    // try to register already existing user
    // try to register with invalid mail 
    // try to register with invalid pass
    // try to register with valid user
    // confirm the registration
    // cancel the registration 

    public class RegisterTests
    {
        private HttpClient _client;
        private TestServer _server;

        public RegisterTests()
        {
            var config = ConfigBuilder.Default.Build();
            _server = TestServerBuilder.BuildServer<Startup>(config);
            _client = _server.CreateClient();
        }

        [Theory(DisplayName = "Try_Register")]

        // try to register already existing user
        [InlineData("alice@localhost", "password", "password", HttpStatusCode.OK, true)]

        // try to register with invalid mail 
        [InlineData("alice_localhost", "password", "password", HttpStatusCode.OK, true)]

        // try to register with invalid pass
        [InlineData("new1@localhost", "password", "does_not_match", HttpStatusCode.OK, true)]
        public async Task Try_Register(
            string email,
            string password,
            string passwordConfirm,
            HttpStatusCode statusCode,
            bool isError)
        {
            // Call the register page 
            var response = await _client.GetAsync($"/register?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password},
                { "PasswordConfirm", passwordConfirm },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() },
                { "ReturnUrl", doc.GetReturnUrl() }
            };

            var response2 = await _client.PostFormAsync(doc.GetFormAction(), form, response);

            if (statusCode == HttpStatusCode.Found)
            {
                // After successfull login user should be redirect to IdentityServer4 authorize endpoint
                response2.StatusCode.Should().Be(HttpStatusCode.Found);
                response2.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
            }
            else
            {
                response2.StatusCode.Should().Be(statusCode);
                var doc2 = await response2.Content.ReadAsHtmlDocumentAsync();

                // Check for error 
                if (isError)
                {
                    var elm = doc2.QuerySelector(".alert.alert-danger");

                    // TODO: check the error message 
                    // elm.TextContent.Contains()
                }
            }
        }
    }

    public class Register2Tests
    {
        [Fact(DisplayName = "Register_With_LoginAfterAccountCreation_False")]
        public async Task Register_With_LoginAfterAccountCreation_False()
        {
            string confirmUrl = null; 
            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountCreated,
                "new2@localhost", 
                (templateName, email, viewData, isHtml) =>
                {
                    // 3. Get confirm url 
                    confirmUrl = viewData.ToDictionary()["ConfirmUrl"] as string;
                });

            // Create a server with custom configuration 
            var config = ConfigBuilder.Default
               .RemoveDefaultMailService() // remove the default service since we mocking it
               .Alter("App:LoginAfterAccountCreation", "false") // dont login after registration
               .Build();

            var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            var client = server.CreateClient();

            // 1. Call the register page 
            var response = await client.GetAsync($"/register?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // 2. Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", "new2@localhost" },
                { "Password", "password" },
                { "PasswordConfirm", "password" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() },
                { "ReturnUrl", doc.GetReturnUrl() }
            };

            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response);

            // 4. You will get redirected to success page
            response2.StatusCode.Should().Be(HttpStatusCode.Found);
            var successUrl = response2.Headers.Location.ToString();
            successUrl.Should().StartWith("/register/success");
            var response3 = await client.GetAsync(successUrl, response2);

            // 5. Confirm the registration by following the link from the email it 
            // should be redirect to IdentityServer4 authorize endpoint
            var response4 = await client.GetAsync(confirmUrl, response3);
            response4.StatusCode.Should().Be(HttpStatusCode.Found);
            response4.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");

            // 6. If the user tries to press the link again he should see the error message 

            // 7. The same applies to cancel link
        }
    }
}

