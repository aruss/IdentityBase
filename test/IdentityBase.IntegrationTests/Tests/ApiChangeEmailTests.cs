namespace IdentityBase.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.TestHost;
    using Moq;
    using Newtonsoft.Json;
    using ServiceBase.Extensions;
    using ServiceBase.Mvc;
    using ServiceBase.Notification.Email;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("ApiChangeEmailTests")]
    public class ApiChangeEmailTests
    {
        const string aliceId = "0c2954d2-4c73-44e3-b0f2-c00403e4adef";
        const string notFoundId = "00000000-4c73-ffe4-b0f2-c00403e4adee";

        private TestServer CreateServer(
            Mock<IEmailService> emailServiceMock = null)
        {
            return TestServerBuilderExtensions.CreateServer(emailServiceMock);
        }

        [Fact(DisplayName = "API: Change email / Confirm / Login")]
        public async Task ChangeEmail_Confirm_Login()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountEmailChanged,
                "nerd@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();

                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{aliceId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri, new
            {
                Email = "nerd@localhost",
                ClientId = "mvc.hybrid",
                Force = false
            });

            response.EnsureSuccessStatusCode();
            // response.AssertSchema(Schemas.InvitationsPostResponse);

            Assert.NotNull(confirmUrl);
            Assert.NotNull(cancelUrl);

            // Post password

            // Try authenticate
        }

        [Fact(DisplayName = "API: Change email / Force update")]
        public async Task ChangeEmail_FoceUpdate()
        {
            string confirmUrl = null;
            string cancelUrl = null;

            // Mock the email service to intercept the outgoing email messages
            var emailServiceMock = EmailServiceHelper.GetEmailServiceMock(
                IdentityBaseConstants.EmailTemplates.UserAccountEmailChanged,
                "nerd@localhost", (templateName, emailTo, viewData, isHtml) =>
                {
                    // 2. Get confirm url and call it
                    confirmUrl = viewData
                        .ToDictionary()["ConfirmUrl"].ToString();

                    cancelUrl = viewData
                        .ToDictionary()["CancelUrl"].ToString();

                });

            TestServer server = this.CreateServer(emailServiceMock);
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{aliceId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri, new
            {
                Email = "nerd@localhost",
                ClientId = "mvc.hybrid",
                Force = true
            });

            response.EnsureSuccessStatusCode();
            var json = response.Content.ReadAsStringAsync().Result;
        }

        [Fact(DisplayName = "API: Change email / User does not exists")]
        public async Task ChangeEmail_UserDoesNotExists()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{notFoundId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri,
                new
                {
                    Email = "nerd@localhost",
                    ClientId = "mvc.hybrid"
                });

            response.StatusCode
                .Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "API: Change email / Email already taken")]
        public async Task ChangeEmail_EmailAlreadyTaken()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{aliceId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri,
                new
                {
                    Email = "bob@localhost",
                    ClientId = "mvc.hybrid",
                });

            response.StatusCode
                .Should().Be(System.Net.HttpStatusCode.BadRequest);

            string json = await response.Content.ReadAsStringAsync();
            json.Should().Be("{\"type\":\"SerializableError\",\"error\":{\"email\":[\"The Email field is invalid, Email already taken.\"]}}");
        }

        [Fact(DisplayName = "API: Change email / ClientId missing")]
        public async Task ChangeEmail_ClientIdMissing()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{aliceId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri,
                new
                {
                    Email = "nerd@localhost",
                });

            response.StatusCode
                .Should().Be(System.Net.HttpStatusCode.BadRequest);

            string json = await response.Content.ReadAsStringAsync();
            json.Should().Be("{\"type\":\"SerializableError\",\"error\":{\"clientId\":[\"The ClientId field is required.\"]}}");
        }

        [Fact(DisplayName = "API: Change email / Invalid ClientId")]
        public async Task ChangeEmail_InvalidClientId()
        {
            TestServer server = this.CreateServer();
            HttpClient client = await server.CreateAuthenticatedClient();

            string uri = $"/api/useraccounts/{aliceId}/change_email";
            HttpResponseMessage response = await client.PostJsonAsync(uri,
                new
                {
                    Email = "nerd@localhost",
                    ClientId = "invalid_one"
                });

            response.StatusCode
                .Should().Be(System.Net.HttpStatusCode.BadRequest);

            string json = await response.Content.ReadAsStringAsync();
            json.Should().Be("{\"type\":\"SerializableError\",\"error\":{\"clientId\":[\"The ClientId field is invalid.\"]}}");
        }
    }
}
