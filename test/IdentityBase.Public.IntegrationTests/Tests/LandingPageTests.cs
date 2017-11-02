namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using ServiceBase.Tests;
    using Xunit;

    [Collection("FooTests")]
    public class LandingPageTests
    {
        [Fact(DisplayName = "Get LandingPage")]
        public async Task Get_LandingPage()
        {
            TestServer server = new TestServerBuilder()
                .UseDefaultSetup()
                .Build();

            HttpClient client = server.CreateClient();
            HttpResponseMessage response = await client.GetAsync("/");
            string html = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
        }
    }
}