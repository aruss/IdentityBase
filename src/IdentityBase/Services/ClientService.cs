
namespace IdentityBase.Services
{
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Http;
    using IdentityBase.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    // TODO: Move to tenant service
    public class ClientService
    {
        private IHttpContextAccessor contextAccessor;
        private IClientStore clientStore;

        public ClientService(
            IClientStore clientStore,
            IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
            this.clientStore = clientStore;
        }

        public async Task<Client> FindEnabledClientByIdAsync(string clientId)
        {
            var client = await clientStore.FindClientByIdAsync(clientId);
            if (client != null && client.Enabled == true)
            {
                return client;
            }

            return null;
        }

        public async Task<IEnumerable<ExternalProvider>>
            GetEnabledProvidersAsync(Client client)
        {
            // TODO: Filter enabled providers by tenant
            var providers = this.contextAccessor.HttpContext
                .Authentication.GetAuthenticationSchemes()

                  .Where(x => x.DisplayName != null)
                  .Select(x => new ExternalProvider
                  {
                      DisplayName = x.DisplayName,
                      AuthenticationScheme = x.AuthenticationScheme
                  });

            if (client != null &&
                client.IdentityProviderRestrictions != null &&
                client.IdentityProviderRestrictions.Any())
            {
                providers = providers.Where(provider =>
                    client.IdentityProviderRestrictions
                        .Contains(provider.AuthenticationScheme));
            }
            
            return providers;
        }
    }
}