// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Services;
    using Microsoft.AspNetCore.Mvc;

    public class AccountController : WebController
    {
        public readonly IUserAccountStore _userAccountStore;

        public AccountController(IUserAccountStore userAccountStore)
        {
            this._userAccountStore = userAccountStore;
        }

        [HttpGet("account", Name = "Account")]
        public async Task<IActionResult> Index()
        {
            var userId = Guid.Parse(HttpContext.User.FindFirst("sub").Value);
            var userAccount = await this._userAccountStore.LoadByIdAsync(userId);

            var vm = new AccountViewModel
            {
                Email = userAccount.Email
            };

            return this.View("Index", vm);
        }

        [HttpPost("account/change-email", Name = "ChangeEmail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail()
        {
            

            return this.RedirectToRoute("Account"); 
        }

        [HttpPost("account/change-password", Name = "ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword()
        {


            return this.RedirectToRoute("Account");
        }
    }
}
