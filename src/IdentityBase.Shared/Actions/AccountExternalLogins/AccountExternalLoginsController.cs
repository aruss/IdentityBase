// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    [Authorize]
    public class AccountExternalLoginsController : WebController
    {
        private readonly AuthenticationService _authenticationService;
        private readonly IUserAccountStore _userAccountStore; 

        public AccountExternalLoginsController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<AccountExternalLoginsController> logger,
            IdentityBaseContext identityBaseContext,
            AuthenticationService authenticationService,
            IUserAccountStore userAccountStore)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._authenticationService = authenticationService;
            this._userAccountStore = userAccountStore; 
        }

        [HttpGet("/account/external-logins", Name = "AccountExternalLogins")]
        public async Task<IActionResult> ExternalLogins()
        {
            List<ExternalProvider> available =
                (await this._authenticationService
                    .GetExternalProvidersAsync())
                    .ToList();

            List<ExternalProvider> connected = new List<ExternalProvider>();

            UserAccount userAccount = await this._authenticationService
                .GetAuthenticatedUserAccountAsync();

            for (int i = available.Count - 1; i >= 0; i--)
            {
                ExternalProvider item = available[i];

                if (userAccount.Accounts
                    .Any(c => c.Provider.Equals(item.AuthenticationScheme)))
                {
                    available.Remove(item);
                    connected.Add(item);
                }
            }

            ExternalLoginsViewModel vm = new ExternalLoginsViewModel
            {
                ClientId = this.IdentityBaseContext.Client.ClientId,
                AvailableProviders = available,
                ConnectedProviders = connected
            };

            return this.View(vm);
        }

        [HttpPost("/account/external-logins/remove", Name = "AccountExternalLoginsRemove")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAccount(
            RemoveLoginInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToInitialAction();
            }

            UserAccount userAccount = await this._authenticationService
                .GetAuthenticatedUserAccountAsync();

            userAccount.Accounts = userAccount.Accounts
                .Where(c => !c.Provider
                    .Equals(
                        inputModel.Provider,
                        StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

            await this._userAccountStore.WriteAsync(userAccount);

            // TODO: Emit user updated event 

            return this.RedirectToInitialAction(); 
        }

        private IActionResult RedirectToInitialAction()
        {
            return this.RedirectToRoute(
                "AccountExternalLogins",
                new { clientId = this.IdentityBaseContext.Client.ClientId }
            );
        }
    }
}
