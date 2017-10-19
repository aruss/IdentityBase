namespace IdentityBase.Public
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using ServiceBase.Notification.Sms;
    using ServiceBase.Notification.Twilio;

    public class TwillioModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DefaultSmsService>().As<ISmsService>();

            builder.RegisterInstance(Current.Configuration
                .GetSection("Sms").Get<DefaultSmsServiceOptions>());

            builder.RegisterType<TwilioSmsSender>().As<ISmsSender>();

            builder.RegisterInstance(Current.Configuration
                .GetSection("Sms:Twillio").Get<TwilioOptions>());
        }
    }
}
