// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using ServiceBase.Notification.Email;

    public class ChangeEmailConfirmController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<ChangeEmailConfirmController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly UserAccountService _userAccountService;
        private readonly IStringLocalizer _localizer;

        public ChangeEmailConfirmController(
            ApplicationOptions applicationOptions,
            ILogger<ChangeEmailConfirmController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._emailService = emailService;
            this._userAccountService = userAccountService;
            this._localizer = localizer;
        }

        [HttpGet("email/confirm/{key}", Name = "EmailConfirm")]
        public async Task<IActionResult> Confirm(string key)
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
    }
}