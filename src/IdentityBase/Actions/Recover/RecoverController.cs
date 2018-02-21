// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class RecoverController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RecoverController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly ClientService _clientService;
        private readonly UserAccountService _userAccountService;
        private readonly NotificationService _notificationService;
        private readonly AuthenticationService _authenticationService;
        private readonly IStringLocalizer _localizer;

        public RecoverController(
            ApplicationOptions applicationOptions,
            ILogger<RecoverController> logger,
            IUserAccountStore userAccountStore,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            ClientService clientService,
            UserAccountService userAccountService,
            NotificationService notificationService,
            AuthenticationService authenticationService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._emailService = emailService;
            this._clientService = clientService;
            this._userAccountService = userAccountService;
            this._notificationService = notificationService;
            this._authenticationService = authenticationService;
            this._localizer = localizer;
        }

        [HttpGet("recover", Name = "Recover")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            RecoverViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this._logger.LogWarning(ErrorMessages.RecoveryNoReturnUrl);

                return this.RedirectToAction("Index", "Error");
            }

            return this.View(vm);
        }

        [HttpPost("recover", Name = "Recover")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RecoverInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(await this.CreateViewModelAsync(model));
            }

            // Check if user with same email exists
            UserAccount userAccount = await this._userAccountService
                .LoadByEmailAsync(model.Email);

            if (userAccount != null)
            {
                if (userAccount.IsLoginAllowed)
                {
                    await this._userAccountService
                        .SetVirificationDataForResetPasswordAsync(
                            userAccount,
                            model.ReturnUrl);

                    await this._notificationService
                        .SendUserAccountRecoverEmailAsync(userAccount);

                    return this.View("Success", new SuccessViewModel()
                    {
                        ReturnUrl = model.ReturnUrl,

                        // TODO: Use a provider helper or something 
                        Provider = userAccount.Email
                            .Split('@')
                            .LastOrDefault()
                    });
                }
                else
                {
                    this.ModelState.AddModelError(this._localizer[
                        ErrorMessages.UserAccountIsDeactivated]);
                }
            }
            else
            {
                this.ModelState.AddModelError(this._localizer[
                    ErrorMessages.UserAccountDoesNotExists]);
            }

            return this.View(
                await this.CreateViewModelAsync(model, userAccount));
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(
                new RecoverInputModel { ReturnUrl = returnUrl }
            );
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(
            RecoverInputModel inputModel,
            UserAccount userAccount = null)
        {
            AuthorizationRequest context = await this._interaction
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            Client client = await this._clientService
                .FindEnabledClientByIdAsync(context.ClientId);

            IEnumerable<ExternalProvider> providers =
                await this._clientService.GetEnabledProvidersAsync(client);

            RecoverViewModel vm = new RecoverViewModel(inputModel)
            {
                EnableAccountRegistration =
                    this._applicationOptions.EnableAccountRegistration,

                EnableLocalLogin = (client != null ?
                    client.EnableLocalLogin : false) &&
                    this._applicationOptions.EnableAccountLogin,

                LoginHint = context.LoginHint,
                ExternalProviders = providers.ToArray(),
                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider)
            };

            return vm;
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

            return this.View("Confirm", vm);
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

            if (this._applicationOptions.CancelAfterAccountRecovery)
            {
                return this.View("Complete");
            }
            else if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                await this._authenticationService
                    .SignInAsync(result.UserAccount, returnUrl);

                return this.RedirectToReturnUrl(returnUrl, this._interaction);
            }   

            return this.RedirectToLogin(returnUrl);
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