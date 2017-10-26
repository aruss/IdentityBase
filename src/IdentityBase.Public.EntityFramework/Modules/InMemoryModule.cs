namespace IdentityBase.Public.EntityFramework
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
                    dbBuilder.UseInMemoryDatabase("");
                };

                configuration.GetSection("EntityFramework").Bind(options);
            });
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}