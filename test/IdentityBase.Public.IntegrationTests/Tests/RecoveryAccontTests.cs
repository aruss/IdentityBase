namespace IdentityBase.Public.IntegrationTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using ServiceBase.Extensions;
    using ServiceBase.Logging;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("FooTests")]
    public class RecoveryAccontTests
    {
        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            return new TestServerBuilder()
                .UseEnvironment("Test")
                .UseContentRoot()
                .AddServices(services =>
                {
                    if (emailServiceMock != null)
                    {
                        services.AddSingleton(emailServiceMock.Object);
                    }
                })
                .AddStartup((environment) =>
                {
                    var builder = new TestConfigurationBuilder()
                        .UseDefaultConfiguration();

                    if (emailServiceMock != null)
                    {
                        builder.RemoveDebugEmailModule();
                    }
                    
                    return new Startup(
                        builder.Build(),
                        environment,
                        new NullLogger<Startup>()
                    );
                })
                .Build();
        }

        [Fact(DisplayName = "Forgot password / Confirm / Add new password / Login")]
        public async Task ForgotPassword_Confirm_AddNewPassword_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
                "alice@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();

                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = server.CreateClient();

            // 1. Call the recovery page and Fill out the form and submit 
            HttpResponseMessage response = await client
                .RecoveryGetAndPostForm("alice@localhost");

            // Wait until we receive the mail 
            do { } while (confirmUrl == null);

            // Call the confirmation link and fill out the form 
            HttpResponseMessage confirmResponse = await client
                       .RecoveryConfirmGetAndPostForm(
                            confirmUrl,
                            "new-password"
                        );

            HttpResponseMessage consentPostResponse = await
                client.ConstentPostForm(false, confirmResponse);

            // Calling confirm url again shouldnt be possible
            await client.RecoveryConfirmGetInvalid(cancelUrl);

            // Calling cancel url shouldnt be possible after successfull
            // confirmation
            await client.RecoveryCancelGetInvalid(cancelUrl);

            HttpResponseMessage loginResponse = await client
                .LoginGetAndPostForm("alice@localhost", "new-password");
        }

        [Fact(DisplayName = "Forgot password / Cancel")]
        public async Task ForgotPassword_Cancel()
        {
            string cancelUrl = null;
            string confirmUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
                "alice@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();
                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = server.CreateClient();

            // Call cancel url
            await client.RecoveryCancelGetValid(cancelUrl);

            // Calling cancel url again shouldnt be possible
            await client.RecoveryCancelGetInvalid(cancelUrl);

            // Calling confirm url shouldnt be possible after successfull
            // cancelation
            await client.RecoveryConfirmGetInvalid(confirmUrl);
        }

        [Theory(DisplayName = "Try_Recover")]

        // Recovering non existing user should return a error message
        [InlineData("nothere@localhost", new string[] {
            IdentityBaseConstants.ErrorMessages.UserAccountDoesNotExists })]

        // Recovering disabled user should return a error message 
        [InlineData("jim@localhost", new string[] {
            IdentityBaseConstants.ErrorMessages.UserAccountIsDeactivated })]

        // Recovering with invalid form values should return a error message 
        [InlineData("alice_localhost", new string[] {
            IdentityBaseConstants.ErrorMessages.InvalidEmailAddress })]

        public async Task Try_Recover(
            string email,
            string[] errorMsgs)
        {
            TestServer server = this.CreateServer();
            HttpClient client = server.CreateClient();

            HttpResponseMessage response = await client
              .RecoveryGetAndPostForm(email);

            IHtmlDocument doc = await response.Content
                .ReadAsHtmlDocumentAsync();

            doc.ShouldContainErrors(errorMsgs);
        }

        [Fact(DisplayName = "Try_Recover_Without_ReturnUrl")]
        public async Task Try_Recover_Without_ReturnUrl()
        {
            TestServer server = this.CreateServer();
            HttpClient client = server.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"/recover");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith("/error");
        }
    }
}