
namespace IdentityBase
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static bool IsAdded<TService>(
            this IServiceCollection services)
        {
            return services.Any(d => d.ServiceType == typeof(TService)); 
        }        
    }
}
