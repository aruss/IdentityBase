using Autofac;
using Autofac.Extensions.DependencyInjection;
using IdentityBase.Public.EntityFramework.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityBase.Public.EntityFramework
{
    public class ExampleDataStoreInitializerModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();
            var config = Current.Configuration;
            var options = new EntityFrameworkOptions();
            Current.Configuration.GetSection("EntityFramework").Bind(options);
            services.AddExampleDataStoreInitializer(options);
            builder.Populate(services);
        }
    }
}
