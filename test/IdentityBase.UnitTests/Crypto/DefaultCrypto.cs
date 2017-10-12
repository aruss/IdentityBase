namespace IdentityBase.UnitTests
{
    using System;
    using IdentityBase.Crypto;
    using Xunit;

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