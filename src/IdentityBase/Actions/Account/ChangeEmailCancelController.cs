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

    public class ChangeEmailController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<ChangeEmailController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly IStringLocalizer _localizer;

        public ChangeEmailController(
            ApplicationOptions applicationOptions,
            ILogger<ChangeEmailController> logger,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._localizer = localizer; 
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