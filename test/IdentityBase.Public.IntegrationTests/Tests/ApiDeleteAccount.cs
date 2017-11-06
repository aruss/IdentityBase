namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using Microsoft.AspNetCore.TestHost;
    using Moq;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("ApiDeleteAccount")]
    public class ApiDeleteAccount
    {
        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            return TestServerBuilderExtensions
                .CreateServer(emailServiceMock, (builder) =>
                {
                    builder
                        .Alter("App:EnableUserAccountDeleteEndpoint", "true");
                }, TestServerBuilderExtensions.CreateServer().CreateHandler());
        }

        [Fact(DisplayName = "API: Delete account / Try login")]
        public async Task DeleteAccount_TryLogin()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = "/useraccounts/0c2954d2-4c73-44e3-b0f2-c00403e4adef";
            HttpResponseMessage deleteResponse = await client.DeleteAsync(uri);
            deleteResponse.EnsureSuccessStatusCode();

            // Try authenticate, should be not possible

            HttpResponseMessage loginResponse = await client
                .LoginGetAndPostFormAsync("alice@localhost", "alice@localhost");

            IHtmlDocument doc = await loginResponse.Content
                .ReadAsHtmlDocumentAsync();

            doc.ShouldContainErrors(
                IdentityBaseConstants.ErrorMessages.InvalidCredentials);
        }
    }
}
