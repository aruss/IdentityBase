namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using Moq;
    using ServiceBase.Extensions;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("ApiChangeEmailTests")]
    public class ApiChangeEmailTests
    {
        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            return TestServerBuilderExtensions
                .CreateServer(emailServiceMock, (builder) =>
                {
                    builder
                        .Alter("App:EnableAccountChangeEmailEndpoint", "true");
                }, TestServerBuilderExtensions.CreateServer().CreateHandler());
        }

        [Fact(DisplayName = "API: Change email / Confirm / Login")]
        public async Task ChangeEmail_Confirm_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountEmailChanged,
                "julia2@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();

                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = "/useraccounts/0c2954d2-4c73-44e3-b0f2-c00403e4adef/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri, new
            {
                Email = "julia2@localhost",
                ClientId = "mvc.hybrid"
            });

            response.EnsureSuccessStatusCode();
            // response.AssertSchema(Schemas.InvitationsPostResponse);

            Assert.NotNull(confirmUrl);
            Assert.NotNull(cancelUrl);

            // Post password

            // Try authenticate
        }
    }
}
