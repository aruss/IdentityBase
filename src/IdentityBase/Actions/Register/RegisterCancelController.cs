// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class RegisterCancelController : WebController
    {
        private readonly ILogger<RegisterCancelController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly IStringLocalizer _localizer;

        public RegisterCancelController(
            ILogger<RegisterCancelController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            IStringLocalizer localizer)
        {
            this._logger = logger;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._localizer = localizer;
        }

        [HttpGet("register/cancel", Name = "RegisterCancel")]
        public async Task<IActionResult> Cancel([FromQuery]string key)
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

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);

            if (this._interaction.IsValidReturnUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }
            else
            {
                return this.RedirectToLogin(returnUrl);
            }
        }
    }
}