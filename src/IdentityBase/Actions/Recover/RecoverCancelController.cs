// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class RecoverCancelController : WebController
    {
        private readonly ILogger<RecoverCancelController> _logger;
        private readonly UserAccountService _userAccountService;
        private readonly IStringLocalizer _localizer;

        public RecoverCancelController(
            ILogger<RecoverCancelController> logger,
            IUserAccountStore userAccountStore,
            UserAccountService userAccountService,
            IStringLocalizer localizer)
        {
            this._logger = logger;
            this._userAccountService = userAccountService;
            this._localizer = localizer;
        }

        [HttpGet("recover/cancel", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel([FromQuery]string key)
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

            return this.RedirectToLogin(returnUrl);
        }
    }
}