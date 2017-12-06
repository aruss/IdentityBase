// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authentication;
    
    // TODO: Move to tenant service
    public class ClientService
    {
        private IHttpContextAccessor _contextAccessor;
        private IClientStore _clientStore;

        public ClientService(
            IClientStore clientStore,
            IHttpContextAccessor contextAccessor)
        {
            this._contextAccessor = contextAccessor;
            this._clientStore = clientStore;
        }

        public async Task<Client> FindEnabledClientByIdAsync(string clientId)
        {
            Client client = await this._clientStore
                .FindClientByIdAsync(clientId);

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
            var providers = this._contextAccessor.HttpContext
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