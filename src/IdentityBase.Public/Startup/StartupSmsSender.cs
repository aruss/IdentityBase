namespace IdentityBase.Public
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Sms;

    public static class StartupSmsSender
    {
        public static void ValidateSmsServices(
            this IServiceCollection services,
            ILogger logger)
        {
            if (!services.IsAdded<ISmsService>())
            {
                throw new Exception("ISmsService not registered.");
            }
        }
    }
}
