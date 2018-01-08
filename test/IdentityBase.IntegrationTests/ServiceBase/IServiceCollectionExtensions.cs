namespace ServiceBase.Extensions
{
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
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