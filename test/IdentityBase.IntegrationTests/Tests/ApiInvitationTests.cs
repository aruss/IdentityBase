namespace IdentityBase.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Moq;
    using ServiceBase.Extensions;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("ApiInvitationTests")]
    public class ApiInvitationTests
    {
        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            return TestServerBuilderExtensions.CreateServer(emailServiceMock);
        }

        [Fact(DisplayName = "API: Invite / Confirm / Add password / Login")]
        public async Task Invite_Confirm_AddPassword_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountInvited,
                "invited@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();

                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = await server.CreateAuthenticatedClient();

            HttpResponseMessage response = await client
                .PutJsonAsync("/api/invitations", new
                {
                    Email = "invited@localhost",
                    ClientId = "mvc.hybrid"
                });

            response.EnsureSuccessStatusCode();
            response.AssertSchema(Schemas.InvitationsPostResponse);

            Assert.NotNull(confirmUrl);
            Assert.NotNull(cancelUrl);

            // Call the confirmation link and fill out the form 
            HttpResponseMessage confirmResponse = await client
                .RegisterConfirmGetAndPostFormAsync(
                    confirmUrl,
                    "supersecret"
                );

            // confirmResponse.ShouldBeRedirectedToAuthorizeEndpoint();
        }
    }
}
