namespace IdentityBase.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
    using Microsoft.AspNetCore.TestHost;
    using ServiceBase.Extensions;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("FooTests")]
    public class RecoveryAccontTests
    {
        [Fact(DisplayName = "Forgot password / Confirm / Add new password / Login")]
        public async Task ForgotPassword_Confirm_AddNewPassword_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                EmailTemplates.UserAccountRecover,
                "alice@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();
                });

            TestServer server = TestServerBuilderExtensions
                .CreateServer(emailServiceMock);

            HttpClient client = server.CreateClient();

            // 1. Call the recovery page and Fill out the form and submit 
            HttpResponseMessage response = await client
                .RecoveryGetAndPostFormAsync("alice@localhost");
            
            Assert.NotNull(confirmUrl);
            Assert.NotNull(cancelUrl);

            // Call the confirmation link and fill out the form 
            HttpResponseMessage confirmResponse = await client
                .RecoveryConfirmGetAndPostFormAsync(
                    confirmUrl,
                    "new-password"
                );

            HttpResponseMessage consentPostResponse = await
                client.ConstentPostFormAsync(false, confirmResponse);

            // Calling confirm url again shouldnt be possible
            await client.RecoveryConfirmGetInvalidAsync(cancelUrl);

            // Calling cancel url shouldnt be possible after successfull
            // confirmation
            await client.RecoveryCancelGetInvalidAsync(cancelUrl);

            HttpResponseMessage loginResponse = await client
                .LoginGetAndPostFormAsync("alice@localhost", "new-password");

            loginResponse.ShouldBeRedirectedToAuthorizeEndpoint();
        }

        [Fact(DisplayName = "Forgot password / Cancel")]
        public async Task ForgotPassword_Cancel()
        {
            string cancelUrl = null;
            string confirmUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                EmailTemplates.UserAccountRecover,
                "alice@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();
                });

            TestServer server = TestServerBuilderExtensions
                .CreateServer(emailServiceMock);

            HttpClient client = server.CreateClient();

            // Call cancel url
            await client.RecoveryCancelGetValidAsync(cancelUrl);

            // Calling cancel url again shouldnt be possible
            await client.RecoveryCancelGetInvalidAsync(cancelUrl);

            // Calling confirm url shouldnt be possible after successfull
            // cancelation
            await client.RecoveryConfirmGetInvalidAsync(confirmUrl);
        }

        [Theory(DisplayName = "Forgot password ")]

        // Recovering non existing user should return a error message
        [InlineData("nothere@localhost", new string[] {
            ErrorMessages.UserAccountDoesNotExists })]

        // Recovering disabled user should return a error message 
        [InlineData("jim@localhost", new string[] {
            ErrorMessages.UserAccountIsDeactivated })]

        // Recovering with invalid form values should return a error message 
        [InlineData("alice_localhost", new string[] {
            ErrorMessages.InvalidEmailAddress })]

        public async Task Try_Recover(
            string email,
            string[] errorMsgs)
        {
            TestServer server = TestServerBuilderExtensions.CreateServer();
            HttpClient client = server.CreateClient();

            HttpResponseMessage response = await client
              .RecoveryGetAndPostFormAsync(email);

            IHtmlDocument doc = await response.Content
                .ReadAsHtmlDocumentAsync();

            doc.ShouldContainErrors(errorMsgs);
        }

        [Fact(DisplayName = "Forgot password / Without returnUrl")]
        public async Task Try_Recover_Without_ReturnUrl()
        {
            TestServer server = TestServerBuilderExtensions.CreateServer();
            HttpClient client = server.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"/recover");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("/error");
        }
    }
}