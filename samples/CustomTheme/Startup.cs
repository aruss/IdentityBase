
// [assembly: Microsoft.Extensions.Configuration.UserSecrets
//     .UserSecretsId("CustomTheme-ce345b64-19cf-4972-b34f-d16f2e7976ee")]

namespace CustomTheme
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Startup class 
    /// </summary>
    public class Startup : IdentityBase.Public.Startup
    {
        /// <summary>
        /// Application entry point 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"><see cref="IHostingEnvironment"/></param>
        /// <param name="logger"><see cref="ILogger"/></param>
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILogger<Startup> logger)
            : base(configuration, environment, logger)
        {

        }
    }
}
