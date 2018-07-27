// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

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

        public AccountMenuViewComponent(IdentityBaseContext idbContext)
        {
            this._idbContext = idbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var vm = new AccountMenuViewModel
            {

                ReturnUrl = this._idbContext.ReturnUrl,
                Items = new AccountMenuItem[]
                {
                    new AccountMenuItem("Account", "AccountProfile", "Profile"),
                    new AccountMenuItem("ChangePassword","AccountChangePassword", "ChangePassword"),
                    new AccountMenuItem("ExternalLogins", "AccountExternalLogins", "ExternalLogins"),
                    new AccountMenuItem("TwoFactorAuthentication", "AccountTwoFactorAuth", "TwoFactorAuth"),
                }
            };

            if (String.IsNullOrWhiteSpace(vm.ReturnUrl))
            {
                vm.ClientId = this._idbContext.Client.ClientId;
            }

            return this.View(vm);
        }
    }
}
