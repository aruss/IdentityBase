// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.Actions.Recover
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class RecoverController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RecoverController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly ClientService _clientService;
        private readonly UserAccountService _userAccountService;

        public RecoverController(
            ApplicationOptions applicationOptions,
            ILogger<RecoverController> logger,
            IUserAccountStore userAccountStore,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            ClientService clientService,
            UserAccountService userAccountService)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._emailService = emailService;
            this._clientService = clientService;
            this._userAccountService = userAccountService;
        }

        [HttpGet("recover", Name = "Recover")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            RecoverViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this._logger.LogWarning(IdentityBaseConstants.ErrorMessages
                    .RecoveryNoReturnUrl);

                return this.Redirect(Url.Action("Index", "Error"));
            }

            return this.View(vm);
        }

        [HttpPost("recover")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RecoverInputModel model)
        {
            if (ModelState.IsValid)
            {
                // Load user by email
                string email = model.Email.ToLower();

                // Check if user with same email exists
                UserAccount userAccount = await this._userAccountService
                    .LoadByEmailAsync(email);

                if (userAccount != null)
                {
                    if (userAccount.IsLoginAllowed)
                    {
                        await this._userAccountService
                            .SetResetPasswordVirificationKeyAsync(
                                userAccount,
                                model.ReturnUrl);

                        await this.SendEmailAsync(userAccount);

                        return this.View("Success", new SuccessViewModel()
                        {
                            ReturnUrl = model.ReturnUrl,
                            Provider = userAccount.Email
                                .Split('@')
                                .LastOrDefault()
                        });
                    }
                    else
                    {
                        this.ModelState.AddModelError(IdentityBaseConstants
                            .ErrorMessages.UserAccountIsDeactivated);
                    }
                }
                else
                {
                    this.ModelState.AddModelError(IdentityBaseConstants
                        .ErrorMessages.UserAccountDoesNotExists);
                }

                return this.View(
                    await this.CreateViewModelAsync(model, userAccount)
                );
            }

            return this.View(await CreateViewModelAsync(model));
        }

        [HttpGet("recover/confirm/{key}", Name = "RecoverConfirm")]
        public async Task<IActionResult> Confirm(string key)
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
                this.ModelState.AddModelError(IdentityBaseConstants
                    .ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            ConfirmViewModel vm = new ConfirmViewModel
            {
                Key = key,
                Email = result.UserAccount.Email
            };

            return this.View(vm);
        }

        [HttpPost("recover/confirm/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmInputModel model)
        {
            TokenVerificationResult result = await this._userAccountService
                .HandleVerificationKeyAsync(
                    model.Key,
                    VerificationKeyPurpose.ResetPassword
                );

            if (result.UserAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                // TODO: clear token if account is there 

                this.ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            if (!ModelState.IsValid)
            {
                return View(new ConfirmViewModel
                {
                    Key = model.Key,
                    Email = result.UserAccount.Email
                });
            }

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService.SetNewPasswordAsync(
                result.UserAccount,
                model.Password
            );

            if (this._applicationOptions.LoginAfterAccountRecovery)
            {
                await this.HttpContext.SignInAsync(result.UserAccount, null);

                if (this._interaction.IsValidReturnUrl(returnUrl))
                {
                    return this.Redirect(returnUrl);
                }
            }

            return this.Redirect(
                this.Url.Action(
                    "Index",
                    "Login",
                    new { ReturnUrl = returnUrl }
                )
            );
        }

        [HttpGet("recover/cancel/{key}", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel(string key)
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
                // TODO: clear token if account is there 
                this.ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return this.View("InvalidToken");
            }

            string returnUrl = result.UserAccount.VerificationStorage;

            await this._userAccountService
                .ClearVerificationAsync(result.UserAccount);

            return this.Redirect(
                this.Url.Action(
                    "Index",
                    "Login",
                    new { ReturnUrl = returnUrl }
                )
            );
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
                    this._applicationOptions.EnableLocalLogin,

                LoginHint = context.LoginHint,
                ExternalProviders = providers.ToArray(),
                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider)
            };

            return vm;
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            string baseUrl = ServiceBase.Extensions.StringExtensions
                .EnsureTrailingSlash(this.HttpContext
                    .GetIdentityServerBaseUrl());

            await this._emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
                userAccount.Email,
                new
                {
                    ConfirmUrl =
                        $"{baseUrl}recover/confirm/{userAccount.VerificationKey}",

                    CancelUrl =
                        $"{baseUrl}recover/cancel/{userAccount.VerificationKey}"
                },
                true
            );
        }
    }
}