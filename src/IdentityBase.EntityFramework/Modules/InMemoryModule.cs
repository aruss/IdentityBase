namespace IdentityBase.EntityFramework
{
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;

    public class InMemoryModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEntityFrameworkStores((options) =>
            {
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder
                        .UseInMemoryDatabase("Put_value_from_config_here");
                };

                configuration.GetSection("EntityFramework").Bind(options);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                EntityFrameworkOptions options = serviceScope.ServiceProvider
                    .GetService<EntityFrameworkOptions>();

                if (options != null)
                {
                    // Disable migration since InMemoryDatabase does not
                    // require one 
                    options.MigrateDatabase = false;
                }
            }            
        }
    }
}