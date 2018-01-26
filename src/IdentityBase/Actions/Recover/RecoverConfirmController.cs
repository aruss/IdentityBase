// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class RecoverConfirmController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RecoverConfirmController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;        
        private readonly AuthenticationService _authenticationService;
        private readonly IStringLocalizer _localizer;

        public RecoverConfirmController(
            ApplicationOptions applicationOptions,
            ILogger<RecoverConfirmController> logger,
            IUserAccountStore userAccountStore,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            AuthenticationService authenticationService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;            
            this._userAccountService = userAccountService;            
            this._authenticationService = authenticationService;
            this._localizer = localizer;
        }
        
        [HttpGet("recover/confirm", Name = "RecoverConfirm")]
        public async Task<IActionResult> Confirm([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ResetPassword
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                this.ModelState.AddModelError(
                    this._localizer[ErrorMessages.TokenIsInvalid]);

                return this.View("InvalidToken");
            }

            ConfirmViewModel vm = new ConfirmViewModel
            {
                Email = result.UserAccount.Email
            };

            return this.View(vm);
        }

        [HttpPost("recover/confirm", Name = "RecoverConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            [FromQuery]string key,
            ConfirmInputModel model)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ResetPassword
                );

            if (result.UserAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                if (result.UserAccount != null)
                {
                    await this._userAccountService
                        .ClearVerificationAsync(result.UserAccount);
                }

                this.ModelState.AddModelError(
                    this._localizer[ErrorMessages.TokenIsInvalid]);

                return this.View("InvalidToken");
            }

            if (!ModelState.IsValid)
            {
                return View(new ConfirmViewModel
                {
                    Email = result.UserAccount.Email
                });
            }

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService.SetNewPasswordAsync(
                result.UserAccount,
                model.Password
            );

            if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                await this._authenticationService
                    .SignInAsync(result.UserAccount, returnUrl);

                return this.RedirectToReturnUrl(returnUrl, this._interaction);
            }

            return this.RedirectToLogin(returnUrl);
        }
    }
}