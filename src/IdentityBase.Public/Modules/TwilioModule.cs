namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Modules;
    using ServiceBase.Notification.Sms;
    using ServiceBase.Notification.Twilio;

    public class TwilioModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<ISmsService, DefaultSmsService>();

            services.AddSingleton(configuration
                .GetSection("Sms").Get<DefaultSmsServiceOptions>());

            services.AddScoped<ISmsSender, TwilioSmsSender>();

            services.AddSingleton(configuration
                .GetSection("Sms:Twilio").Get<TwilioOptions>());
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
