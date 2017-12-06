namespace IdentityBase.EntityFramework
{
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
            services.AddTransient<ConfigBasedStoreInitializer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ConfigBasedStoreInitializer initializer = serviceScope
                    .ServiceProvider
                    .GetService<ConfigBasedStoreInitializer>();

                if (initializer != null)
                {
                    initializer.InitializeStores();
                }
            }
        }
    }
}