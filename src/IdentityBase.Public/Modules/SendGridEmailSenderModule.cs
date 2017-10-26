namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Notification.Email;
    using ServiceBase.Notification.SendGrid;

    public class SendGridEmailSenderModule : IModule
    {
        public void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IEmailService, DefaultEmailService>(); 

            services.AddSingleton(configuration.GetSection("Email")
                .Get<DefaultEmailServiceOptions>());

            services.AddScoped<IEmailSender, SendGridEmailSender>(); 

            services.AddSingleton(configuration
                .GetSection("Email:SendGrid").Get<SendGridOptions>());
        }

        public void Configure(IApplicationBuilder app)
        {

        }
    }
}
