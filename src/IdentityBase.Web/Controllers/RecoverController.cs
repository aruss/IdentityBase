// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Forms;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityBase.Shared.InputModels.Recover;
    using IdentityBase.Web;
    using IdentityBase.Web.ViewModels.Recover;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Mvc;
    using ServiceBase.Notification.Email;

    public class RecoverController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly IEmailService _emailService;
        private readonly ClientService _clientService;
        private readonly UserAccountService _userAccountService;
        private readonly NotificationService _notificationService;
        private readonly AuthenticationService _authenticationService;

        public RecoverController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<RecoverController> logger,
            ApplicationOptions applicationOptions,
            IUserAccountStore userAccountStore,
            IEmailService emailService,
            ClientService clientService,
            UserAccountService userAccountService,
            NotificationService notificationService,
            AuthenticationService authenticationService)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this._applicationOptions = applicationOptions;
            this._emailService = emailService;
            this._clientService = clientService;
            this._userAccountService = userAccountService;
            this._notificationService = notificationService;
            this._authenticationService = authenticationService;
        }

        [HttpGet("recover", Name = "Recover")]
        [RestoreModelState]
        public async Task<IActionResult> Recover(string returnUrl)
        {
            RecoverViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this.Logger.LogWarning(ErrorMessages.RecoveryNoReturnUrl);

                return this.RedirectToAction("Index", "Error");
            }

            CreateViewModelResult result =
                await this.CreateViewModel<IRecoverCreateViewModelAction>();

            vm.FormElements = result.FormElements;

            return this.View(vm);
        }

        [HttpPost("recover", Name = "Recover")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> Recover(RecoverInputModel model)
        {
            BindInputModelResult formResult =
                await this.BindInputModel<IRecoverBindInputModelAction>();

            if (!ModelState.IsValid)
            {
                return this.RedirectToAction(
                    "Recover",
                    "Recover",
                    new { ReturnUrl = model.ReturnUrl }
                );
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

                    return this.View("Success", new SuccessViewModel
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
                    this.AddModelError(ErrorMessages.UserAccountIsDeactivated);
                }
            }
            else
            {
                this.AddModelError(ErrorMessages.UserAccountDoesNotExists);
            }

            // there was an error
            return this.RedirectToAction(
                "Recover",
                "Recover",
                new { ReturnUrl = model.ReturnUrl });
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
            AuthorizationRequest context = await this.InteractionService
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            Client client = await this._clientService
                .FindEnabledClientByIdAsync(context.ClientId);

            IEnumerable<ExternalProvider> providers =
                await this._clientService.GetEnabledProvidersAsync(client);

            RecoverViewModel vm = new RecoverViewModel
            {
                Email = inputModel.Email,
                ReturnUrl = inputModel.ReturnUrl,

                EnableAccountRegistration =
                    this._applicationOptions.EnableAccountRegistration,

                EnableLocalLogin = (client != null ?
                    client.EnableLocalLogin : false) &&
                    this._applicationOptions.EnableAccountLogin,

                LoginHint = context.LoginHint,
                ExternalProviders = providers.Select(s =>
                    new Web.ViewModels.External.ExternalProvider
                    {
                        AuthenticationScheme = s.AuthenticationScheme,
                        DisplayName = s.DisplayName
                    }).ToArray(),
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
                this.AddModelError(ErrorMessages.TokenIsInvalid);

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

                this.AddModelError(ErrorMessages.TokenIsInvalid);

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

                return this.RedirectToReturnUrl(returnUrl);
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

                this.AddModelError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);

            return this.RedirectToLogin(returnUrl);
        }
    }
}