// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System.Threading.Tasks;
    using IdentityBase.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Authorize]
    public class AccountTwoFactorAuthController : WebController
    {
        public AccountTwoFactorAuthController()
        {

        }

        [HttpGet("/account/two-factor-auth", Name = "AccountTwoFactorAuth")]
        public async Task<IActionResult> TwoFactorAuth()
        {

            return this.View(); 
        }
    }
}
