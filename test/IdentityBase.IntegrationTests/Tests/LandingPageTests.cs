namespace IdentityBase.IntegrationTests
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.TestHost;
    using Xunit;
    using FluentAssertions; 

    [Collection("FooTests")]
    public class LandingPageTests
    {
        [Fact(DisplayName = "Get LandingPage")]
        public async Task Get_LandingPage()
        {
            TestServer server = TestServerBuilderExtensions.CreateServer();

            HttpClient client = server.CreateClient();
            HttpResponseMessage response = await client.GetAsync("/");
            
            response.StatusCode.Should()
                .BeEquivalentTo(System.Net.HttpStatusCode.NotFound); 
        }
    }
}