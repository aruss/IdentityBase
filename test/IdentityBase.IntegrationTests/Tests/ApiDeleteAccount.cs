namespace IdentityBase.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using AngleSharp.Dom.Html;
    using FluentAssertions;
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
            return TestServerBuilderExtensions.CreateServer(emailServiceMock);
        }

        [Fact(DisplayName = "API: Delete account / Try login")]
        public async Task DeleteAccount_TryLogin()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = "/api/useraccounts/0c2954d2-4c73-44e3-b0f2-c00403e4adef";
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

        [Fact(DisplayName = "API: Delete account / User does not exists")]
        public async Task DeleteAccount_UserDoesNotExists()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = "/api/useraccounts/1c2954d2-4c73-44e3-b0f2-c00403e4adee";
            HttpResponseMessage response = await client.DeleteAsync(uri);

            response.StatusCode
                .Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
