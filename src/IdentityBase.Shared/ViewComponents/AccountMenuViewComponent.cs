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
        public AccountMenuItem(string name, string controller, string action)
        {
            this.Name = name;
            this.Controller = controller;
            this.Action = action;
        }

        public string Name { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
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
                    new AccountMenuItem(
                        "Account",
                        "AccountProfile",
                        "Profile"
                    ),

                    new AccountMenuItem(
                        "ChangePassword",
                        "AccountChangePassword",
                        "ChangePassword"
                    )
               };

            var accountProviders = await _authenticationService
                .GetExternalProvidersAsync();

            if (accountProviders.Any())
            {
                items.Add(new AccountMenuItem(
                    "ExternalLogins",
                    "AccountExternalLogins",
                    "ExternalLogins"
                ));
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
