// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class RegisterConfirmController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RegisterConfirmController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly AuthenticationService _authenticationService;
        private readonly IStringLocalizer _localizer;

        public RegisterConfirmController(
            ApplicationOptions applicationOptions,
            ILogger<RegisterConfirmController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
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

        [HttpGet("register/confirm", Name = "RegisterConfirm")]
        public async Task<IActionResult> Confirm([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
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

            // User account requires completion.
            if (this._applicationOptions.EnableAccountInvitation &&
                result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                // TODO: move invitation confirmation to own contoller
                //       listening on /invitation/confirm

                ConfirmViewModel vm = new ConfirmViewModel
                {
                    RequiresPassword = !result.UserAccount.HasPassword(),
                    Email = result.UserAccount.Email
                };

                return this.View(vm);
            }
            // User account already fine and can be authenticated.
            else
            {
                // TODO: Refactor so the db will hit only once in case
                //       LoginAfterAccountConfirmation is true

                string returnUrl = result.UserAccount.VerificationStorage;

                await this._userAccountService
                    .SetEmailVerifiedAsync(result.UserAccount);

                if (this._applicationOptions.LoginAfterAccountConfirmation)
                {
                    await this._authenticationService
                        .SignInAsync(result.UserAccount, returnUrl);

                    return this.RedirectToReturnUrl(
                        returnUrl, this._interaction);
                }

                return this.RedirectToLogin(returnUrl);
            }
        }

        // Currently is only used for invitations 
        [HttpPost("register/confirm", Name = "RegisterConfirm")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(
            [FromQuery]string key,
            ConfirmInputModel model)
        {
            if (!this._applicationOptions.EnableAccountInvitation)
            {
                return this.NotFound();
            }

            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ConfirmAccount
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

            if (!this.ModelState.IsValid)
            {
                return this.View(new ConfirmViewModel
                {
                    Email = result.UserAccount.Email
                });
            }

            string returnUrl = result.UserAccount.VerificationStorage;
            this._userAccountService.SetEmailVerified(result.UserAccount);

            this._userAccountService
                .AddLocalCredentials(result.UserAccount, model.Password);

            await this._userAccountService
                .UpdateUserAccountAsync(result.UserAccount);

            if (result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                return this.RedirectToReturnUrl(
                        returnUrl, this._interaction);
            }
            else
            {
                if (this._applicationOptions.LoginAfterAccountRecovery)
                {
                    await this._authenticationService
                        .SignInAsync(result.UserAccount, returnUrl);

                    return this.RedirectToReturnUrl(
                        returnUrl, this._interaction);
                }

                return this.RedirectToLogin(returnUrl);
            }
        }   
    }
}