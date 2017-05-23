using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBase.Events;
using System;

namespace IdentityBase.Public
{
    public static class StartupEvents
    {
        public static void ValidateEventServices(this IContainer container, ILogger logger)
        {
            if (!container.IsRegistered<IEventService>()) { throw new Exception("IEventService not registered."); }
        }
    }

    public class DefaultEventModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(Current.Configuration.GetSection("Events").Get<EventOptions>() ?? new EventOptions());
            builder.RegisterType<DefaultEventService>().As<IEventService>();
            builder.RegisterType<DefaultEventSink>().As<IEventSink>();
        }
    }
}
