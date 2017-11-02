namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Email;

    public class DebugEmailModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IEmailService, DebugEmailService>(); 
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
