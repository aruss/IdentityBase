using IdentityBase.Crypto;
using System;
using Xunit;

namespace IdentityBase.UnitTests
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