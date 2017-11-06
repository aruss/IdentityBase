namespace IdentityBase.Public.IntegrationTests
{
    using System.Net.Http;
    using System.Text;
    using ServiceBase.Tests;

    public static class MockHttpMessageHandlerBuilderExtensions
    {
        public static MockHttpMessageHandlerBuilder AddOpenidConfiguration(
            this MockHttpMessageHandlerBuilder builder)
        {
            return builder.AddEndpoint(
                "/.well-known/openid-configuration",
                () =>
                {
                    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{}", Encoding.UTF8, "application/json")
                    };
                    return response;
                }
            );
        }
    }
}

