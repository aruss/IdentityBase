namespace IdentityBase.EntityFramework.InMemory
{
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder app)
        {
            using (IServiceScope serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                EntityFrameworkOptions options = serviceScope.ServiceProvider
                    .GetService<EntityFrameworkOptions>();

                if (options != null)
                {
                    // Disable migration since InMemoryDatabase does not
                    // require one 
                    options.MigrateDatabase = false;
                }
            }
        }
    }
}
