// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Forms;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Extensions;
    using ServiceBase.Mvc;
    using ServiceBase.Notification.Email;

    public class RecoverController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly IEmailService _emailService;
        private readonly IUserAccountStore _userAccountStore;
        private readonly UserAccountService _userAccountService;
        private readonly NotificationService _notificationService;
        private readonly AuthenticationService _authenticationService;
        private readonly IEmailProviderInfoService _emailProviderInfoService;

        public RecoverController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<RecoverController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            IEmailService emailService,
            IUserAccountStore userAccountStore,
            UserAccountService userAccountService,
            NotificationService notificationService,
            AuthenticationService authenticationService,
            IEmailProviderInfoService emailProviderInfoService)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._emailService = emailService;
            this._userAccountStore = userAccountStore;
            this._userAccountService = userAccountService;
            this._notificationService = notificationService;
            this._authenticationService = authenticationService;
            this._emailProviderInfoService = emailProviderInfoService;
        }

        [HttpGet("/recover", Name = "Recover")]
        [RestoreModelState]
        public async Task<IActionResult> Recover(string returnUrl)
        {
            RecoverViewModel vm = await this.CreateViewModelAsync(returnUrl);

            return this.View(vm);
        }

        [HttpPost("/recover", Name = "Recover")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> Recover(RecoverInputModel model)
        {
            BindInputModelResult formResult = await this.
                BindFormInputModelAsync<IRecoverBindInputModelAction>();

            if (!ModelState.IsValid)
            {
                return this.RedirectToRoute(
                    "Recover",
                    new { ReturnUrl = model.ReturnUrl }
                );
            }
                        
            // Check if user with same email exists
            UserAccount userAccount = await this._userAccountStore
                .LoadByEmailAsync(model.Email);

            if (userAccount != null)
            {
                if (userAccount.IsActive)
                {
                    this._userAccountService.SetVerificationData(
                            userAccount,
                            VerificationKeyPurpose.ResetPassword,
                            model.ReturnUrl);

                    await this._userAccountStore.WriteAsync(userAccount);

                    await this._notificationService
                        .SendUserAccountRecoverEmailAsync(userAccount);

                    return this.View("Success", new SuccessViewModel
                    {
                        ReturnUrl = model.ReturnUrl,

                        Provider = await this._emailProviderInfoService
                            .GetProviderInfo(userAccount.Email)
                    });
                }
                else
                {
                    // TODO: return propper view model instead of input model with apropriate flags 
                    this.AddModelStateError(
                        nameof(RecoverInputModel.Email),
                        ErrorMessages.UserAccountIsInactive);
                }
            }
            else
            {
                // TODO: return propper view model instead of input model with apropriate flags 
                this.AddModelStateError(
                    nameof(RecoverInputModel.Email),
                    ErrorMessages.UserAccountDoesNotExists);
            }

            // there was an error
            return this.RedirectToRoute(
                "Recover",
                new { ReturnUrl = model.ReturnUrl });
        }

        [NonAction]
        private async Task<RecoverViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(
                new RecoverInputModel { ReturnUrl = returnUrl }
            );
        }

        [NonAction]
        private async Task<RecoverViewModel> CreateViewModelAsync(
            RecoverInputModel inputModel,
            UserAccount userAccount = null)
        {
            AuthorizationRequest context = await this.InteractionService
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            Client client = this.IdentityBaseContext.Client;

            //IEnumerable<ExternalProvider> providers =
            //    await this._clientService.GetEnabledProvidersAsync(client);

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

                ExternalProviders = await this._authenticationService
                    .GetExternalProvidersAsync(),

                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider)
            };

            vm.FormModel = await this
                .CreateFormViewModelAsync<IRecoverCreateViewModelAction>(vm);

            return vm;
        }

        [HttpGet("/recover/confirm", Name = "RecoverConfirm")]
        [RestoreModelState]
        public async Task<IActionResult> Confirm([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ResetPassword
                );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            ConfirmViewModel vm = new ConfirmViewModel
            {
                Email = result.UserAccount.Email
            };

            return this.View("Confirm", vm);
        }

        [HttpPost("/recover/confirm", Name = "RecoverConfirm")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> Confirm(
            [FromQuery]string key,
            ConfirmInputModel model)
        {
            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ResetPassword
                );

            UserAccount userAccount = result.UserAccount;

            if (userAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                if (userAccount != null)
                {
                    this._userAccountService
                        .ClearVerificationData(userAccount);

                    await this._userAccountStore
                        .WriteAsync(userAccount);

                    // TODO: Emit user updated event 
                }

                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            if (!ModelState.IsValid)
            {
                return this.RedirectToRoute("RecoverConfirm", new {
                    key = key,
                    clientId = this.Request.Query["clientId"],
                    culture = this.Request.Query["culture"]
                });
            }

            string returnUrl = userAccount.VerificationStorage;

            this._userAccountService.SetPassword(
                userAccount,
                model.Password
            );

            if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                await this._authenticationService
                    .SignInAsync(userAccount, returnUrl);

                this._userAccountService.SetSuccessfullSignIn(userAccount);

                // TODO: Emit user authenticated event
            }

            await this._userAccountStore.WriteAsync(userAccount);

            // TODO: Emit user updated eventd

            if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                return this.RedirectToReturnUrl(returnUrl);
            }

            return this.RedirectToLogin(returnUrl);
        }

        [HttpGet("/recover/cancel", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel([FromQuery]string key)
        {
            TokenVerificationResult result = await this._userAccountService
                .GetVerificationResultAsync(
                    key,
                    VerificationKeyPurpose.ResetPassword
                );

            UserAccount userAccount = result.UserAccount;

            if (userAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                if (userAccount != null)
                {
                    this._userAccountService
                        .ClearVerificationData(userAccount);

                    await this._userAccountStore
                        .WriteAsync(userAccount);
                }

                this.AddModelStateError(ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            string returnUrl = userAccount.VerificationStorage;

            this._userAccountService
                .ClearVerificationData(userAccount);

            await this._userAccountStore.WriteAsync(userAccount);

            return this.RedirectToLogin(returnUrl);
        }
    }
}