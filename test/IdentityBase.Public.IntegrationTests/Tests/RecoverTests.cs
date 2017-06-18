using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using ServiceBase.Extensions;
using ServiceBase.Notification.Email;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    [Collection("Recover Tests")]
    public class RecoverTests
    {
        private HttpClient _client;
        private TestServer _server;

        public RecoverTests()
        {
            // Create strict moq so it throws exceptions if get called, since this
            // tests should all fail 
            var emailServiceMock = new Mock<IEmailService>(MockBehavior.Strict);

            // Create a server with custom configuration 
            var config = ConfigBuilder.Default
               // remove the default service since we mocking it
               .RemoveDefaultMailService()
               .Build();

            _server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            _client = _server.CreateClient();
        }

        [Theory(DisplayName = "Try_Recover")]

        // Recovering non existing user should return a error message
        [InlineData("nothere@localhost", true, new string[] { IdentityBaseConstants.ErrorMessages.UserAccountDoesNotExists })]

        // Recovering disabled user should return a error message 
        [InlineData("jim@localhost", true, new string[] { IdentityBaseConstants.ErrorMessages.UserAccountIsDeactivated })]

        // Recovering with invalid form values should return a error message 
        [InlineData("alice_localhost", true, new string[] { IdentityBaseConstants.ErrorMessages.InvalidEmailAddress })]
        public async Task Try_Recover(string email, bool isError, string[] errorMsgs)
        {
            // Call the recovery page 
            var response = await _client.GetAsync($"/recover?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", email },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var response2 = await _client.PostFormAsync(doc.GetFormAction(), form, response);
            var doc2 = await response2.Content.ReadAsHtmlDocumentAsync();
            doc2.ShouldContainErrors(errorMsgs);
        }

        [Fact(DisplayName = "Try_Recover_Without_ReturnUrl")]
        public async Task Try_Recover_Without_ReturnUrl()
        {
            // Call the recovery page without returnUrl parameter 
            var response = await _client.GetAsync($"/recover");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("/error");
        }
    }

    [Collection("Recover Tests")]
    public class Recover2Tests
    {
        private async Task<HttpResponseMessage> GetAndPostRecoverForm(
            bool loginAfterAccountRecovery,
            Action<TestServer, HttpClient> gotServer,
            Action<string, string> gotMail)
        {
            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
                "alice@localhost",
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
               .Alter("App:LoginAfterAccountRecovery", loginAfterAccountRecovery ? "true" : "false")
               .Build();

            var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            var client = server.CreateClient();

            gotServer(server, client);

            // Call the recovery page 
            var response = await client.GetAsync(
                $"/recover?returnUrl={Constants.ReturnUrl}");
            response.EnsureSuccessStatusCode();

            // Fill out the form and submit 
            var doc = await response.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Email", "alice@localhost" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };

            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response);
            response2.EnsureSuccessStatusCode();
            
            return response2;
        }

        /// <summary>
        /// - Recover existing, enabled user should display a success message
        /// - Send a email message with confirm and cancel links 
        /// - If user confirms the recovery a password edit form should be displayed 
        ///     - User submits a new password and saves the form 
        ///     - User is not authenticated after successfull form submit but is redirect to login page
        ///     - User can login with new password
        ///     - User cannot confirm twice
        ///     - User cannot cancel recovery
        /// </summary>
        /// <returns></returns>
        [Fact(DisplayName = "Recover_Confirm_Change_Password_LoginAfterAccountRecovery_False")]
        public async Task Recover_Confirm_Change_Password_LoginAfterAccountRecovery_False()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRecoverForm(
                false,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            // Follow the confirmation link from the mail 
            var response1 = await client.GetAsync(confirmUrl, response);
            response1.EnsureSuccessStatusCode();
            var doc = await response1.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Password", "new-password" },
                { "PasswordConfirm", "new-password" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };
            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response1);
            response2.StatusCode.Should().Be(HttpStatusCode.Found);
            response2.Headers.Location.ToString().Should().StartWith("/login?ReturnUrl=");

            // User should be able to login 
            await (new LoginTests().Try_Login_With_Local_Account("alice@localhost", "new-password", false, false));

            // Try to follow the confirmation link again it should return an error 
            var response3 = await client.GetAsync(confirmUrl, response2);
            response3.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc2 = await response3.Content.ReadAsHtmlDocumentAsync();
            doc2.ShouldContainErrors(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

            // The same should apply to cancel url
            response3 = await client.GetAsync(cancelUrl, response2);
            response3.StatusCode.Should().Be(HttpStatusCode.OK);
            doc2 = await response3.Content.ReadAsHtmlDocumentAsync();
            doc2.ShouldContainErrors(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
        }

        [Fact(DisplayName = "Recover_Confirm_Change_Password_LoginAfterAccountRecovery_True")]
        public async Task Recover_Confirm_Change_Password_LoginAfterAccountRecovery_True()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRecoverForm(
                true,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            // Follow the confirmation link from the mail 
            var response1 = await client.GetAsync(confirmUrl, response);
            response1.EnsureSuccessStatusCode();
            var doc = await response1.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Password", "new-password" },
                { "PasswordConfirm", "new-password" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };
            var response2 = await client.PostFormAsync(doc.GetFormAction(), form, response1);
            response2.StatusCode.Should().Be(HttpStatusCode.Found);
            response2.Headers.Location.ToString().Should().StartWith("/connect/authorize/login");
        }

        [Fact(DisplayName = "Recover_Cancel")]
        public async Task Recover_Cancel()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRecoverForm(
                false,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            // Follow the confirmation link from the mail 
            var response1 = await client.GetAsync(cancelUrl, response);
            response1.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response1.Headers.Location.ToString().Should().StartWith("/login");
        }

        [Fact(DisplayName = "Recover_Confirm_With_Wrong_Key")]
        public async Task Recover_Confirm_With_Wrong_Key()
        {
            string confirmUrl = null;
            string cancelUrl = null;
            TestServer server = null;
            HttpClient client = null;

            var response = await GetAndPostRecoverForm(
                false,
                (a, b) => { server = a; client = b; },
                (a, b) => { confirmUrl = a; cancelUrl = b; }
            );

            // Follow the confirmation link from the mail 
            var response1 = await client.GetAsync(confirmUrl, response);
            response1.EnsureSuccessStatusCode();
            var doc = await response1.Content.ReadAsHtmlDocumentAsync();
            var form = new Dictionary<string, string>
            {
                { "Password", "new-password" },
                { "PasswordConfirm", "new-password" },
                { "__RequestVerificationToken", doc.GetAntiForgeryToken() }
            };
            var response2 = await client.PostFormAsync(doc.GetFormAction() + "fo", form, response1);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc2 = await response2.Content.ReadAsHtmlDocumentAsync();
            doc2.ShouldContainErrors(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
        }
    }
}

