// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.UserAccounts
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;

    public class UserAccountsController : WebApiController
    {
        private readonly UserAccountService _userAccountService;

        public UserAccountsController(
            UserAccountService userAccountService)
        {
            this._userAccountService = userAccountService;
        }
        
        [HttpDelete("useraccounts/{UserAccountId}")]
        [ScopeAuthorize(WebApiConstants.ApiName, AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Delete([FromRoute]Guid userAccountId)
        {
            UserAccount userAccount = await this._userAccountService
                .LoadByIdAsync(userAccountId);

            if (userAccount == null)
            {
                return this.NotFound();
            }
            
            await this._userAccountService.DeleteByIdAsync(userAccountId);

            return this.Ok(); 
        }
    }
}