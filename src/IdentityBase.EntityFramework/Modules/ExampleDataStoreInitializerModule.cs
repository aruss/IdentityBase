namespace IdentityBase.EntityFramework
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;

    public class ExampleDataStoreInitializerModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddTransient<ExampleDataStoreInitializer>();
        }

        public void Configure(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                ExampleDataStoreInitializer initializer = serviceScope
                    .ServiceProvider
                    .GetService<ExampleDataStoreInitializer>();

                if (initializer != null)
                {
                    initializer.InitializeStores();
                }
            }
        }
    }
}