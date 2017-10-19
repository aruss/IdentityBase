namespace IdentityBase.Public
{
    using Autofac;
    using ServiceBase.Notification.Email;

    public class DebugEmailModule : Autofac.Module
    {
        /// <summary>
        /// Loads dependencies 
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DebugEmailService>().As<IEmailService>();
        }
    }
}
