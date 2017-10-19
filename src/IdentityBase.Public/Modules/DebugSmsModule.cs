namespace IdentityBase.Public
{
    using Autofac;
    using ServiceBase.Notification.Sms;

    public class DebugSmsModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DebugSmsService>().As<ISmsService>();
        }
    }
}
