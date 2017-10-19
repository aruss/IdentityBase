using Autofac;
using Microsoft.Extensions.Configuration;
using ServiceBase.Events;

namespace IdentityBase.Public
{
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
