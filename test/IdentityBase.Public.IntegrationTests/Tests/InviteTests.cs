namespace IdentityBase.Public.IntegrationTests.Tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;
    using Xunit;

    [Collection("Login Tests")]
    public class InviteTests
    {
        [Fact(DisplayName = "Invite_User 2")]
        public async Task Invite_User2()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages.
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountInvited,
                "invited@localhost",
                (templateName, emailTo, viewData, isHtml) =>
                {
                    // 3. Get confirm url 
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"] as string;

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"] as string;
                });

            // Create a server with custom configuration.
            var config = ConfigBuilder.Default
               // Remove the default service since we mocking it.
               .RemoveDefaultMailService()
               .Alter("App:EnableInvitationCreateEndpoint", "true");

            await TestServerBuilder2.BuildServerAsync(async (server) =>
            {
                HttpClient client = server.CreateClient();
                
                HttpResponseMessage response = await client
                    .PutJsonAsync("/invitations", new
                    {
                        Email = "invited@localhost",
                        ClientId = "mvc.hybrid"
                    });

                string json = await response.Content.ReadAsStringAsync();

                response.EnsureSuccessStatusCode();

                // Try to follow the confirmation link again it should return an error.
                HttpResponseMessage response3 =
                    await client.GetAsync(confirmUrl);

                response3.StatusCode.Should().Be(HttpStatusCode.OK);

                IHtmlDocument doc2 =
                    await response3.Content.ReadAsHtmlDocumentAsync();                

            }, config, (services)  =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
        }
        
        [Fact(DisplayName = "Invite_User")]
        public async Task Invite_User()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountInvited,
                "invited@localhost",
                (templateName, emailTo, viewData, isHtml) =>
                {
                    // 3. Get confirm url 
                    confirmUrl = viewData.ToDictionary()["ConfirmUrl"] as string;
                    cancelUrl = viewData.ToDictionary()["CancelUrl"] as string;
                });

            // Create a server with custom configuration 
            var config = ConfigBuilder.Default
               // remove the default service since we mocking it
               .RemoveDefaultMailService()
               .Alter("App:EnableInvitationCreateEndpoint", "true")
               .Build();

            var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            var client = server.CreateClient();

            // Act
            var response = await client.PutJsonAsync("/invitations", new
            {
                Email = "invited@localhost",
                ClientId = "mvc.hybrid"
            });
            response.EnsureSuccessStatusCode();

            // Try to follow the confirmation link again it should return an error 
            var response3 = await client.GetAsync(confirmUrl);
            response3.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc2 = await response3.Content.ReadAsHtmlDocumentAsync();
        }
    }
}
