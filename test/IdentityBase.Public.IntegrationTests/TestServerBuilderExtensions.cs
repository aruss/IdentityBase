namespace IdentityBase.Public.IntegrationTests
{
    using System.IO;
    using IdentityBase.Public;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging.Abstractions;
    using ServiceBase.Tests;

    public static class TestServerBuilderExtensions
    {
        public static TestServerBuilder UseContentRoot(
            this TestServerBuilder builder)
        {
            string contentRoot = Path.GetFullPath(Path.Combine(
              Directory.GetCurrentDirectory(), "src/IdentityBase.Public"));

            if (!Directory.Exists(contentRoot))
            {
                contentRoot = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    "../../../../../src/IdentityBase.Public"));
            }

            if (!Directory.Exists(contentRoot))
            {
                throw new DirectoryNotFoundException(contentRoot);
            }

            return builder.UseContentRoot(contentRoot);
        }

        public static TestServerBuilder UseDefaultSetup(
            this TestServerBuilder builder)
        {
            return builder
                .UseEnvironment("Test")
                .UseContentRoot()
                .AddStartup((environment) =>
                {
                    IConfiguration config = new TestConfigurationBuilder()
                        .UseDefaultConfiguration()
                        .Build();

                    return new Startup(
                        config,
                        environment,
                        new NullLogger<Startup>()
                    );
                }); 
        }
    }
}
