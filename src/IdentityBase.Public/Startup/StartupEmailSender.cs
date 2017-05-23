using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Email;
using ServiceBase.Notification.SendGrid;
using ServiceBase.Notification.Smtp;
using System;

namespace IdentityBase.Public
{
    public static class StartupEmailSender
    {
        public static void ValidateEmailSenderServices(this IContainer container, ILogger logger)
        {
            if (!container.IsRegistered<IEmailService>()) { throw new Exception("IEmailService not registered."); }
        }
    }

    public class SmtpEmailSenderModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultEmailService>().As<IEmailService>();
            builder.RegisterInstance(Current.Configuration.GetSection("Email").Get<DefaultEmailServiceOptions>());
            builder.RegisterType<SmtpEmailSender>().As<IEmailSender>();
            builder.RegisterInstance(Current.Configuration.GetSection("Email:Smtp").Get<SmtpOptions>());
        }
    }

    public class SendGridEmailSenderModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultEmailService>().As<IEmailService>();
            builder.RegisterInstance(Current.Configuration.GetSection("Email").Get<DefaultEmailServiceOptions>());
            builder.RegisterType<SendGridEmailSender>().As<IEmailSender>();
            builder.RegisterInstance(Current.Configuration.GetSection("Email:SendGrid").Get<SendGridOptions>());
        }
    }


    public class DebugEmailModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DebugEmailService>().As<IEmailService>();
        }
    }
}
