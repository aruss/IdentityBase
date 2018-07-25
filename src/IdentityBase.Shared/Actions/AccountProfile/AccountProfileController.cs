// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
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
    [Route("/account")]
    [RestoreModelState]
    public class AccountProfileController : WebController
    {
        private readonly UserAccountService _userAccountService;
        private readonly AuthenticationService _authService;

        public AccountProfileController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<AccountProfileController> logger,
            IdentityBaseContext identityBaseContext,
            UserAccountService userAccountService,
            AuthenticationService authService)

        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._userAccountService = userAccountService;
            this._authService = authService;
        }

        [HttpGet(Name = "account")]
        [RestoreModelState]
        public async Task<IActionResult> Profile()
        {
            UserAccount userAccount = await this._authService
                .GetAuthenticatedUserAccountAsync();

            ProfileViewModel vm = new ProfileViewModel
            {
                Email = userAccount.Email,
                // Phone = userAccount.Phone,
                ReturnUrl = this.IdentityBaseContext.ReturnUrl
            };

            return this.View(vm);
        }

        [HttpPost(Name = "account")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> Profile(ProfileInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.RedirectToInitialAction();
            }

            // TODO: update stuff here 

            return this.RedirectToInitialAction(); 
        }

        private RedirectToActionResult RedirectToInitialAction()
        {
            return this.RedirectToAction(
                "Profile",
                "AccountProfile",
                new { ReturnUrl = this.IdentityBaseContext.ReturnUrl }
            );
        }
    }
}
