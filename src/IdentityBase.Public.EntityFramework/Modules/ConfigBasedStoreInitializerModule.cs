namespace IdentityBase.Public.EntityFramework
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using IdentityBase.Public.EntityFramework.Options;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class ConfigBasedStoreInitializerModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            ServiceCollection services = new ServiceCollection();
            IConfiguration config = Current.Configuration;
            EntityFrameworkOptions options = new EntityFrameworkOptions();
            Current.Configuration.GetSection("EntityFramework").Bind(options);
            services.AddConfigBasedStoreInitializer(options);
            builder.Populate(services);
        }
    }
}