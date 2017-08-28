using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    [Collection("Register Tests")]
    public class RegisterTests
    {

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
            var config = ConfigBuilder.Default.Build();
            var server = TestServerBuilder.BuildServer<Startup>(config);
            var client = server.CreateClient();

            // Call the register page 
            var response = await client.GetAsync($"/register?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", email },
                { "Password", password},
                { "PasswordConfirm", passwordConfirm },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response);

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

                    throw new NotImplementedException();

                    // TODO: check the error message 
                    // elm.TextContent.Contains()
                }
            }
        }
    }

    [Collection("Register Tests")]
    public class Register2Tests
    {
        private async Task<HttpResponseMessage> GetAndPostRegisterForm(
           bool loginAfterAccountCreation,
           bool loginAfterAccountConfirmation,
           Action<TestServer, HttpClient> gotServer,
           Action<string, string> gotMail)
        {
            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountCreated,
                "newone@localhost",
                (templateName, emailTo, viewData, isHtml) =>
                {
                    // 3. Get confirm url 
                    var confirmUrl = viewData.ToDictionary()["ConfirmUrl"] as string;
                    var cancelUrl = viewData.ToDictionary()["CancelUrl"] as string;

                    gotMail(confirmUrl, cancelUrl);
                });

            // Create a server with custom configuration 
            var config = ConfigBuilder.Default
               // remove the default service since we mocking it
               .RemoveDefaultMailService()
               // dont login after recovery
               .Alter("App:LoginAfterAccountCreation", loginAfterAccountCreation ? "true" : "false")
               .Alter("App:LoginAfterAccountConfirmation", loginAfterAccountConfirmation ? "true" : "false")
               .Build();

            var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            var client = server.CreateClient();

            gotServer(server, client);

            // Call the recovery page 
            var response = await client.GetAsync($"/register?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", "newone@localhost" },
                { "Password", "newone@localhost" },
                { "PasswordConfirm", "newone@localhost" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response);
            response2.EnsureSuccessStatusCode();

            //if (!loginAfterAccountCreation)
            //{
                return response2;
            //}
            //else
            //{
            //
            //}
        }

        [Fact(DisplayName = "Register_LoginAfterAccountCreation_False_LoginAfterAccountConfirmation_False")]
        public async Task Register_LoginAfterAccountCreation_False_LoginAfterAccountConfirmation_False()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRegisterForm(
                false,
                false,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            var response1 = await client.GetAsync(confirmUrl, response);
            response1.StatusCode.Should().Be(HttpStatusCode.Found);
            response1.Headers.Location.ToString().Should().StartWith("/login");

            var response2 = await client.GetAsync(confirmUrl, response);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc = await response2.Content.ReadAsHtmlDocumentAsync();
            doc.ShouldContainErrors(IdentityBaseConstants.ErrorMessages.TokenIsInvalid); 
        }

        [Fact(DisplayName = "Register_LoginAfterAccountCreation_False_LoginAfterAccountConfirmation_True")]
        public async Task Register_LoginAfterAccountCreation_False_LoginAfterAccountConfirmation_True()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRegisterForm(
                false,
                true,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            var response1 = await client.GetAsync(confirmUrl, response);
            response1.StatusCode.Should().Be(HttpStatusCode.Found);
            response1.Headers.Location.ToString().Should().StartWith("/login");
        }
    }
}

