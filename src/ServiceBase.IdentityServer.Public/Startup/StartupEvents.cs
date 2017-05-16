using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.Events;
using System;

namespace ServiceBase.IdentityServer.Public
{
    public static class StartupEvents
    {
        public static void AddEvents(this IServiceCollection services, IConfigurationRoot config, ILogger logger, IHostingEnvironment environment)
        {
            if (!String.IsNullOrWhiteSpace(config["Events"]))
            {
                services.AddSingleton(config.GetSection("Events").Get<EventOptions>());
                services.AddTransient<IEventService, DefaultEventService>();
                services.AddTransient<IEventSink, DefaultEventSink>();

                if (!String.IsNullOrWhiteSpace(config["Events:Logstash"]))
                {
                    // TODO: configure logstash event sink 
                }
            }
            else
            {
                logger.LogInformation("No event sing registered");
            }
        }
    }
}
