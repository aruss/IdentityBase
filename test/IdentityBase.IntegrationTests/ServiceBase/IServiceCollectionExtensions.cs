namespace ServiceBase.Extensions
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection TryRemove<TService>(
           this IServiceCollection services)
        {
            ServiceDescriptor descriptorToRemove = services
               .FirstOrDefault(d => d.ServiceType == typeof(TService));

            if (descriptorToRemove != null)
            {
                services.Remove(descriptorToRemove);
            }

            return services;
        }

        public static IServiceCollection Replace<TService>(
             this IServiceCollection services,
             TService implementation)
             where TService : class
        {
            services.TryRemove<TService>();

            ServiceDescriptor descriptorToAdd = new ServiceDescriptor(
                typeof(TService),
                implementation);

            services.Add(descriptorToAdd);

            return services;
        }
    }
}