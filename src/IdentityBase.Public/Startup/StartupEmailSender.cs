namespace IdentityBase.Public
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public static class StartupEmailSender
    {
        public static void ValidateEmailSenderServices(
            this IServiceCollection services,
            ILogger logger)
        {
            if (!services.IsAdded<IEmailService>())
            {
                throw new Exception("IEmailService not registered.");
            }
        }
    }
}
