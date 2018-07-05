// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Web.Controllers.Account
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityBase.Web.ViewModels.Account;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class AccountController : WebController
    {
        private readonly UserAccountService _userAccountService;

        public AccountController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<AccountController> logger,
            UserAccountService userAccountService)
            
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this._userAccountService = userAccountService;
        }

        [HttpGet("account", Name = "Account")]
        public async Task<IActionResult> Account()
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
        public Task<IActionResult> ChangeEmail()
        {
            return Task.FromResult<IActionResult>(
                this.RedirectToRoute("Account")); 
        }

        [HttpPost("account/change-password", Name = "ChangePassword")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ChangePassword()
        {
            return Task.FromResult<IActionResult>(
                this.RedirectToRoute("Account")); 
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
                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

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
                this.AddModelStateError(ErrorMessages.EmailAddressAlreadyTaken);

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

            return this.RedirectToReturnUrl(returnUrl); 
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

                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);

            string[] storage = JsonConvert.DeserializeObject<string[]>(
                result.UserAccount.VerificationStorage);

            string email = storage[0];
            string returnUrl = storage[1];

            return this.RedirectToReturnUrl(returnUrl);           
        }
    }
}
