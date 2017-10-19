namespace IdentityBase.Public
{
    using System;
    using Autofac;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Sms;

    public static class StartupSmsSender
    {
        public static void ValidateSmsServices(
            this IContainer container,
            ILogger logger)
        {
            if (!container.IsRegistered<ISmsService>()) {
                throw new Exception("ISmsService not registered."); }
        }
    }
}
