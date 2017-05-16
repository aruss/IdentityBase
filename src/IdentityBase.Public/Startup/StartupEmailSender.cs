using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Email;
using ServiceBase.Notification.SendGrid;
using ServiceBase.Notification.Smtp;
using System;

namespace IdentityBase.Public
{
    public static class StartupEmailSender
    {
        public static void AddEmailSender(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            if (!String.IsNullOrWhiteSpace(config["Email"]))
            {
                services.Configure<DefaultEmailServiceOptions>(config.GetSection("Email"));

                if (!String.IsNullOrWhiteSpace(config["Email:Smtp"]))
                {
                    services.AddTransient<IEmailService, DefaultEmailService>();
                    services.AddSingleton(config.GetSection("Email:Smtp").Get<SmtpOptions>());
                    services.AddTransient<IEmailSender, SmtpEmailSender>();
                }

                else if (!String.IsNullOrWhiteSpace(config["Email:SendGrid"]))
                {
                    services.AddTransient<IEmailService, DefaultEmailService>();
                    services.AddSingleton(config.GetSection("Email:SendGrid").Get<SendGridOptions>());
                    services.AddTransient<IEmailSender, SendGridEmailSender>();
                }

                // else if o360
                // else if MailGun
            }
            else
            {
                logger.LogError("Email service configuration not present");
                if (environment.IsDevelopment())
                {
                    services.AddTransient<IEmailService, DebugEmailService>();
                }
                else
                {
                    throw new Exception("Email service configuration not present");
                }
            }
        }
    }
}
