namespace IdentityBase.Public.EntityFramework
{
    using IdentityBase.Public.EntityFramework.Options;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;

    public class ConfigBasedStoreInitializerModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var options = new EntityFrameworkOptions();
            configuration.GetSection("EntityFramework").Bind(options);
            services.AddConfigBasedStoreInitializer(options);            
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}