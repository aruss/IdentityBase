namespace IdentityBase.Public
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using ServiceBase.Notification.Email;
    using ServiceBase.Notification.Smtp;

    public class SmtpEmailSenderModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultEmailService>().As<IEmailService>();

            builder.RegisterInstance(Current.Configuration
                .GetSection("Email").Get<DefaultEmailServiceOptions>());

            builder.RegisterType<SmtpEmailSender>().As<IEmailSender>();

            builder.RegisterInstance(Current.Configuration
                .GetSection("Email:Smtp").Get<SmtpOptions>());
        }
    }
}
