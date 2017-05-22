using IdentityServer4.Models;
using IdentityBase.Models;
using System.Collections.Generic;
using System;

namespace IdentityBase.Services
{
    public interface IStoreInitializer
    {
        void InitializeStores();
        void CleanupStores();
    }
}
