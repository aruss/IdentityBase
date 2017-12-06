namespace IdentityBase.EntityFramework.MySql
{
    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;

    public class MySqlModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration config)
        {
            services.AddEntityFrameworkStores((options) =>
            {
                string migrationsAssembly =
                    typeof(MySqlModule)
                    .GetTypeInfo().Assembly.GetName().Name;

                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseMySql(
                        config["EntityFramework:MySql:ConnectionString"],
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