using IdentityServer4.Models;
using ServiceBase.IdentityServer.Models;
using System.Collections.Generic;
using System;

namespace ServiceBase.IdentityServer.Services
{
    public interface IStoreInitializer
    {
        void InitializeStores();
    }
}
