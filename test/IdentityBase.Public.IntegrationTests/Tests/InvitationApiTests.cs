namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using ServiceBase.Extensions;
    using ServiceBase.Logging;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("InvitationApiTests")]
    public class InvitationApiTests
    {
        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            Startup startup = null;

            var server = new TestServerBuilder()
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
                        .UseDefaultConfiguration()
                        .Alter("App:EnableInvitationCreateEndpoint", "true");

                    if (emailServiceMock != null)
                    {
                        builder.RemoveDebugEmailModule();
                    }

                    startup = new Startup(
                        builder.Build(),
                        environment,
                        new NullLogger<Startup>(),
                        CreateTokenServer().CreateHandler()
                    );

                    return startup;
                })
                .Build();

            return server;
        }

        private TestServer CreateTokenServer()
        {
            return new TestServerBuilder()
                .UseEnvironment("Test")
                .UseContentRoot()
                .AddStartup((environment) =>
                {
                    return new Startup(
                        new TestConfigurationBuilder()
                            .UseDefaultConfiguration().Build(),
                        environment,
                        new NullLogger<Startup>()
                    );
                })
                .Build();
        }

        [Fact(DisplayName = "Invite / Confirm / Add password / Login")]
        public async Task Invite_Confirm_AddPassword_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
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
                .PutJsonAsync("/invitations", new
                {
                    Email = "invited@localhost",
                    ClientId = "mvc.hybrid"
                });

            response.EnsureSuccessStatusCode();

            response.AssertSchema(@"{
                'type': 'object',
                'additionalProperties' : false,
                'properties': {
                'id': {
                    'type': 'string'
                },
                'email': {
                    'type': [
                    'string',
                    'null'
                    ]
                },
                'createdAt': {
                    'type': 'string',
                    'format': 'date-time'
                },
                'verificationKeySentAt': {
                    'type': 'string',
                    'format': 'date-time'      
                }
                },
                'required': [
                    'id',
                    'email',
                    'createdAt',
                    'verificationKeySentAt'
                ]
            }");
        }
    }
}