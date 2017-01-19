using ServiceBase.IdentityServer.Crypto;
using System;
using Xunit;

namespace ServiceBase.IdentityServer.UnitTests
{
    [Collection("ICrypto")]
    public class DefaultCryptoTests : IDisposable
    {
        private DefaultCrypto crypto;

        public DefaultCryptoTests()
        {
            crypto = new DefaultCrypto();
        }

        public void Dispose()
        {
        }
    }
}