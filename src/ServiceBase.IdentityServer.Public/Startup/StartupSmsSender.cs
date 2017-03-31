using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Sms;
using ServiceBase.Notification.Twilio;
using System;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupSmsSender
    {
        public static void AddSmsSender(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            if (String.IsNullOrWhiteSpace(config["Twillio"]))
            {
                services.AddTransient<ISmsService, DefaultSmsService>();
                services.Configure<TwillioOptions>(config.GetSection("Twillio"));
                services.AddTransient<ISmsSender, TwillioSmsSender>();
            }
            // TODO: Add additional services here
            else
            {

                logger.LogError("SMS service configuration not present");
                if (environment.IsDevelopment())
                {
                    services.AddTransient<ISmsService, DebugSmsService>();
                }
                else
                {
                    throw new Exception("SMS service configuration not present");
                }
            }
        }
    }
}
