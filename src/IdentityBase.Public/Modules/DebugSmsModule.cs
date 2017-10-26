namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Notification.Sms;

    public class DebugSmsModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISmsService, DebugSmsService>();
        }

        public void Configure(IApplicationBuilder app)
        {

        }        
    }
}
