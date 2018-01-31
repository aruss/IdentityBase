// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Newtonsoft.Json;

    public class AccountController : WebController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IStringLocalizer _localizer;
        private readonly IIdentityServerInteractionService _interaction;

        public AccountController(
            UserAccountService userAccountService,
            IStringLocalizer localizer,
            IIdentityServerInteractionService interaction)
        {
            this._userAccountService = userAccountService;
            this._localizer = localizer;
            this._interaction = interaction; 
        }

        [HttpGet("account", Name = "Account")]
        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(HttpContext.User.FindFirst("sub").Value);

            UserAccount userAccount = await this._userAccountService
                .LoadByIdAsync(userId);

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


        [HttpGet("email/confirm", Name = "EmailConfirm")]
        public async Task<IActionResult> Confirm([FromQuery]string key)
        {
            // Check token integrity
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ChangeEmail
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                this.ModelState.AddModelError(
                    this._localizer[ErrorMessages.TokenIsInvalid]);

                return this.View("InvalidToken");
            }

            // TODO: Move to verification storage reader or something
            string[] storage = JsonConvert.DeserializeObject<string[]>(
                result.UserAccount.VerificationStorage);

            string email = storage[0];
            string returnUrl = storage[1];

            // Check if new email address is already taken
            if (await this._userAccountService
                .LoadByEmailAsync(email) != null)
            {
                this.ModelState.AddModelError(
                    this._localizer[ErrorMessages.EmailAddressAlreadyTaken]);

                var vm = new ChangeEmailConfirmViewModel
                {
                    Key = key,
                    ReturnUrl = returnUrl,
                    Email = result.UserAccount.Email
                };

                return this.View(vm);
            }

            // Update user account if email still available
            await this._userAccountService.SetNewEmailAsync(
               result.UserAccount,
               email
            );

            if (this._interaction.IsValidReturnUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            throw new ApplicationException("Invalid return URL");
        }

        [HttpGet("email/cancel", Name = "EmailCancel")]
        public async Task<IActionResult> Cancel([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    key,
                    VerificationKeyPurpose.ChangeEmail
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

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);

            string[] storage = JsonConvert.DeserializeObject<string[]>(
                result.UserAccount.VerificationStorage);

            string email = storage[0];
            string returnUrl = storage[1];

            if (this._interaction.IsValidReturnUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            throw new ApplicationException("Invalid return URL");
        }
    }
}
