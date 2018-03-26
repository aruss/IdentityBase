namespace IdentityBase.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;

    // Landing page does not exists anymore
    // 
    // [Collection("FooTests")]
    // public class LandingPageTests
    // {
    //     [Fact(DisplayName = "Get LandingPage")]
    //     public async Task Get_LandingPage()
    //     {
    //         TestServer server = TestServerBuilderExtensions.CreateServer();
    //
    //         HttpClient client = server.CreateClient();
    //         HttpResponseMessage response = await client.GetAsync("/");
    //         string html = await response.Content.ReadAsStringAsync();
    //         response.EnsureSuccessStatusCode();
    //     }
    // }
}