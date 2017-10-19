namespace IdentityBase.Public
{
    using System;
    using Autofac;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public static class StartupEmailSender
    {
        public static void ValidateEmailSenderServices(
            this IContainer container,
            ILogger logger)
        {
            if (!container.IsRegistered<IEmailService>())
            {
                throw new Exception("IEmailService not registered.");
            }
        }
    }
}
