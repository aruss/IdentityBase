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
    using Microsoft.AspNetCore.Authentication;

    public class ClientService
    {
        private IClientStore _clientStore;
        private IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public ClientService(
            IClientStore clientStore,
            IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            this._clientStore = clientStore;
            this._authenticationSchemeProvider = authenticationSchemeProvider;
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
            IEnumerable<ExternalProvider> providers =
                (await this._authenticationSchemeProvider.GetAllSchemesAsync())
                .Where(x => x.DisplayName != null)
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
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