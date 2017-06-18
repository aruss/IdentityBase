using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace IdentityBase.Public.IntegrationTests.Tests
{
    [Collection("Login Tests")]
    public class InviteTests
    {
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
                .Alter("App:EnableUserInviteEndpoint", "true")
               .Build();

            var server = TestServerBuilder.BuildServer<Startup>(config, (services) =>
            {
                services.AddSingleton(emailServiceMock.Object);
            });
            var client = server.CreateClient();

            // Act
            var response = await client.PutJsonAsync("/api/invitations", new
            {
                Email = "invited@localhost",
                ClientId = "mvc"
            });
            response.EnsureSuccessStatusCode();


            // Try to follow the confirmation link again it should return an error 
            var response3 = await client.GetAsync(confirmUrl);
            response3.StatusCode.Should().Be(HttpStatusCode.OK);
            var doc2 = await response3.Content.ReadAsHtmlDocumentAsync();

        }
    }
}
