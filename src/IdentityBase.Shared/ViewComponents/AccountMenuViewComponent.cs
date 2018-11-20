// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using IdentityBase.Services;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using IdentityBase.Models;

    public class AccountMenuViewComponent : ViewComponent
    {
        private readonly IdentityBaseContext _idbContext;
        private readonly AuthenticationService _authenticationService;

        public AccountMenuViewComponent(
            IdentityBaseContext idbContext,
            AuthenticationService authenticationService)
        {
            this._idbContext = idbContext;
            this._authenticationService = authenticationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = new List<AccountMenuItem>
            {
                new AccountMenuItem("AccountProfile"),
                new AccountMenuItem("AccountChangePassword")
            };

            IEnumerable<ExternalProvider> accountProviders =
                await this._authenticationService.GetExternalProvidersAsync();

            if (accountProviders.Any())
            {
                items.Add(new AccountMenuItem("AccountExternalLogins"));
            }

            // Add two factor auth if any of thow factor services are installed

            //new AccountMenuItem("TwoFactorAuthentication", "AccountTwoFactorAuth", "TwoFactorAuth"),

            // TODO: add logout button to the main menu 

            var vm = new AccountMenuViewModel
            {
                ReturnUrl = this._idbContext.ReturnUrl,
                Items = items.ToArray()
            };

            if (String.IsNullOrWhiteSpace(vm.ReturnUrl))
            {
                vm.ClientId = this._idbContext?.Client?.ClientId;
            }

            return this.View(vm);
        }
    }
}
