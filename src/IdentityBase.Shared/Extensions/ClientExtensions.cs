namespace IdentityBase.Extensions
{
    using System;
    using System.Linq;
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using ServiceBase.Extensions;

    public static partial class ClientExtensions
    {
        public static ClientProperties GetClientProperties(this Client client)
        {
            return client.Properties.ToObject<ClientProperties>();
        }

        public static string TryGetReturnUri(
            this Client client,
            string returnUri)
        {
            if (String.IsNullOrWhiteSpace(returnUri) &&
                client.RedirectUris.Count > 0)
            {
                return client.RedirectUris.First();
            }
            else if (client.RedirectUris.Contains(returnUri))
            {
                return returnUri;
            }

            return null;
        }
    }
}
