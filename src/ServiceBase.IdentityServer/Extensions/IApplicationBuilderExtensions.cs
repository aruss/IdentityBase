using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceBase.IdentityServer.Services;

namespace ServiceBase.IdentityServer.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder InitializeStores(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var initializer = serviceScope.ServiceProvider.GetService<IStoreInitializer>();
                if (initializer != null)
                {
                    initializer.InitializeStores();
                }
            }

            return app;
        }
    }
}
