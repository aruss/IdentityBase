using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityBase.Public.EntityFramework
{
    public class InMemoryModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            var config = Current.Configuration;

            services.AddEntityFrameworkStores((options) =>
            {
                options.DbContextOptions = (dbBuilder) =>
                {
                    dbBuilder.UseInMemoryDatabase();
                };

                config.GetSection("EntityFramework").Bind(options);
            });

            builder.Populate(services);
        }
    }
}
