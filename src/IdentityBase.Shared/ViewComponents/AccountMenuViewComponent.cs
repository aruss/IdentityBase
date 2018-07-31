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

    public class AccountMenuViewModel
    {
        public string ReturnUrl { get; set; }
        public string ClientId { get; set; }
        public IEnumerable<AccountMenuItem> Items { get; set; }
    }

    public class AccountMenuItem
    {
        public AccountMenuItem(string route, string name = null)
        {
            this.Route = route;
            this.Name = name ?? route;
        }

        public string Route { get; set; }
        public string Name { get; set; }
    }

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

            var accountProviders = await _authenticationService
                .GetExternalProvidersAsync();

            if (accountProviders.Any())
            {
                items.Add(new AccountMenuItem("AccountExternalLogins"));
            }

            // Add two factor auth if any of thow factor services are installed

            //new AccountMenuItem("TwoFactorAuthentication", "AccountTwoFactorAuth", "TwoFactorAuth"),

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
