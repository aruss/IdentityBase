namespace IdentityBase.EntityFramework.Npgsql
{
    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;

    public class NpgsqlModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration config)
        {
            services.AddEntityFrameworkStores((options) =>
            {
                string migrationsAssembly =
                    typeof(NpgsqlModule)
                    .GetTypeInfo().Assembly.GetName().Name;

                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseNpgsql(
                        config["EntityFramework:Npgsql:ConnectionString"],
                        o => o.MigrationsAssembly(migrationsAssembly)
                    );
                };

                config.GetSection("EntityFramework").Bind(options);
            });
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }   
}