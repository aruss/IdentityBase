namespace IdentityBase.IntegrationTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.TestHost;

    public static class TestServerExtensions
    {
        public static DiscoveryClient CreateDiscoveryClient(
            this TestServer testServer)
        {
            return new DiscoveryClient(
                testServer.BaseAddress.ToString(),
                testServer.CreateHandler()
            );
        }

        public static TokenClient CreateTokenClient(
            this TestServer testServer,
            string TokenEndpoint,
            string clientId,
            string clientSecret)
        {
            return new TokenClient(
                TokenEndpoint,
                clientId,
                clientSecret,
                testServer.CreateHandler()
            );
        }

        public static async Task<TokenResponse> RequestTokenAsync(
            this TestServer testServer,
            string clientId,
            string clientSecret,
            string scope = null)
        {
            DiscoveryClient discoveryClient =
                testServer.CreateDiscoveryClient();

            DiscoveryResponse discoResult =
                await discoveryClient.GetAsync();

            if (discoResult.IsError)
            {
                throw new Exception(discoResult.Error);
            }

            TokenClient tokenClient = testServer.CreateTokenClient(
                discoResult.TokenEndpoint,
                clientId,
                clientSecret);

            return await tokenClient.RequestClientCredentialsAsync(scope);
        }

        public static async Task<HttpClient> CreateAuthenticatedClient(
            this TestServer testServer)
        {
            TokenResponse tokenResponse = await testServer.RequestTokenAsync(
                  "client", // Some api client 
                  "secret",
                  "idbase"
              );

            if (tokenResponse.IsError)
            {
                throw new Exception(tokenResponse.Error);
            }

            HttpClient client = testServer.CreateClient();

            client.SetBearerToken(tokenResponse.AccessToken);

            return client;
        }
    }
}
