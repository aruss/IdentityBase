namespace IdentityBase.Extensions
{
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using ServiceBase.Extensions;

    public static partial class ClientExtensions
    {
        public static ClientProperties GetClientProperties(this Client client)
        {
            return client.Properties.ToObject<ClientProperties>();
        }
    }
}
