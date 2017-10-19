namespace IdentityBase.Public
{
    using System;
    using Autofac;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Events;

    public static class StartupEvents
    {
        public static void ValidateEventServices(
            this IContainer container,
            ILogger logger)
        {
            if (!container.IsRegistered<IEventService>())
            {
                throw new Exception("IEventService not registered.");
            }
        }
    }
}
