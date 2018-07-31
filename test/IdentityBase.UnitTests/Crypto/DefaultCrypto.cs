namespace IdentityBase.UnitTests
{
    using System;
    using IdentityBase.Crypto;
    using Xunit;

    [Collection("ICryptoService")]
    public class DefaultCryptoTests : IDisposable
    {
        private DefaultCryptoService _cryptoService;

        public DefaultCryptoTests()
        {
            this._cryptoService = new DefaultCryptoService();
        }

        public void Dispose()
        {
        }
    }
}