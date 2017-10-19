namespace IdentityBase.Public
{
    using System;
    using Autofac;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.Extensions.Logging;

    public static class StartupDataLayer
    {
        public static void ValidateDataLayerServices(
            this IContainer container,
            ILogger logger)
        {
            if (container.IsRegistered<IStoreInitializer>())
            {
                logger.LogInformation("IStoreInitializer registered.");
            }

            if (!container.IsRegistered<IClientStore>())
            {
                throw new Exception("IClientStore not registered.");
            }

            if (!container.IsRegistered<IResourceStore>())
            {
                throw new Exception("IResourceStore not registered.");
            }

            if (!container.IsRegistered<ICorsPolicyService>())
            {
                throw new Exception("ICorsPolicyService not registered.");
            }

            if (!container.IsRegistered<IPersistedGrantStore>())
            {
                throw new Exception("IPersistedGrantStore not registered.");
            }

            if (!container.IsRegistered<IUserAccountStore>())
            {
                throw new Exception("IUserAccountStore not registered.");
            }
        }
    }
}
