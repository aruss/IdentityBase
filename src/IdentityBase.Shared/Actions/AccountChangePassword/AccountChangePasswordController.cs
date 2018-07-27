// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.AccountChangePassword
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Mvc;

    [Authorize]
    [Route("/account/change-password")]
    public class AccountChangePasswordController : WebController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IUserAccountStore _userAccountStore;
        private readonly AuthenticationService _authService;

        public AccountChangePasswordController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<AccountChangePasswordController> logger,
            IdentityBaseContext identityBaseContext,
            UserAccountService userAccountService,
            IUserAccountStore userAccountStore,
            AuthenticationService authService)

        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._userAccountService = userAccountService;
            this._userAccountStore = userAccountStore;
            this._authService = authService;
        }

        [HttpGet(Name = "AccountChangePassword")]
        [RestoreModelState]
        public async Task<IActionResult> ChangePassword()
        {
            UserAccount userAccount = await this._authService
                           .GetAuthenticatedUserAccountAsync();

            ChangePasswordViewModel vm = new ChangePasswordViewModel
            {
                ReturnUrl = this.IdentityBaseContext.ReturnUrl
            };

            return this.View(vm);
        }

        [HttpPost(Name = "AccountChangePassword")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToInitialAction();
            }

            UserAccount userAccount = await this._authService
                .GetAuthenticatedUserAccountAsync();

            if (!this._userAccountService.IsPasswordValid(
                userAccount.PasswordHash,
                inputModel.PasswordCurrent))
            {
                this.AddModelStateError("PasswordCurrent",
                    "Current password does not match");

                return this.RedirectToInitialAction();
            }

            this._userAccountService
                .SetPassword(userAccount, inputModel.Password);

            await this._userAccountStore.WriteAsync(userAccount);

            // TODO: Emit user updated event 

            return this.RedirectToInitialAction();
        }

        private RedirectToActionResult RedirectToInitialAction()
        {
            return this.RedirectToAction(
                "ChangePassword",
                "AccountChangePassword",
                new { ReturnUrl = this.IdentityBaseContext.ReturnUrl }
            );
        }
    }
}
