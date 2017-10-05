using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Sms;
using ServiceBase.Notification.Twilio;
using System;

namespace IdentityBase.Public
{
    public static class StartupSmsSender
    {
        public static void ValidateSmsServices(this IContainer container, ILogger logger)
        {
            if (!container.IsRegistered<ISmsService>()) { throw new Exception("ISmsService not registered."); }
        }
    }

    public class TwillioModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultSmsService>().As<ISmsService>();
            builder.RegisterInstance(Current.Configuration.GetSection("Sms").Get<DefaultSmsServiceOptions>());
            builder.RegisterType<TwilioSmsSender>().As<ISmsSender>();
            builder.RegisterInstance(Current.Configuration.GetSection("Sms:Twillio").Get<TwilioOptions>());
        }
    }

    public class DebugSmsModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DebugSmsService>().As<ISmsService>();
        }
    }
}
