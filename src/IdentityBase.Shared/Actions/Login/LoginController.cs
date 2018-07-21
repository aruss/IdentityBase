// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Forms;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Mvc;

    public class LoginController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly UserAccountService _userAccountService;
        private readonly AuthenticationService _authenticationService;

        public LoginController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<LoginController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            UserAccountService userAccountService,
            AuthenticationService authenticationService)
        {
            // setting it this way since interaction service is null in the
            // base class oO
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._userAccountService = userAccountService;
            this._authenticationService = authenticationService;
        }

        /// <summary>
        /// Shows the login page with local and external logins.
        /// </summary>
        [HttpGet("login", Name = "Login")]
        [RestoreModelState]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel vm = await this.CreateViewModelAsync(returnUrl);

            // If local authentication is disbaled and there is only one
            // external provider provided so do just external authentication
            // without showing the login page 
            if (vm.IsExternalLoginOnly)
            {
                return this.RedirectToAction("ExternalChallenge", new
                {
                    provider = vm.ExternalProviders
                        .First().AuthenticationScheme,

                    returnUrl = returnUrl
                });
            }

            vm.FormModel =
                await this.CreateViewModel<ILoginCreateViewModelAction>(vm);

            return this.View(vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost("login", Name = "Login")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            // TODO: extract in own controller and not add it 
            if (!this._applicationOptions.EnableAccountLogin)
            {
                return this.NotFound();
            }

            BindInputModelResult formResult =
               await this.BindInputModel<ILoginBindInputModelAction>();

            // invalid input (return to same login)
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToLogin(model.ReturnUrl);
            }

            UserAccountVerificationResult verificationResult =
                await this._userAccountService.VerifyByEmailAndPasswordAsync(
                    model.Email,
                    model.Password
                );

            // user not present (wrong email)
            if (verificationResult.UserAccount == null)
            {
                // Show email or password is invalid
                this.AddModelStateError(ErrorMessages.InvalidCredentials);
                return this.RedirectToLogin(model.ReturnUrl);
            }

            // User may not login (deactivated)
            if (!verificationResult.IsLoginAllowed)
            {
                // If paranoya mode is on, do not epose any existence
                // of user prensentce 
                if (this._applicationOptions.ObfuscateUserAccountPresence)
                {
                    // And then send an email with info what actually happened.
                    throw new NotImplementedException(
                        "Send email with information that user account is disabled."
                    );

                    // Show "email or password is invalid" message
                    this.AddModelStateError(ErrorMessages.InvalidCredentials);
                }
                else
                {
                    this.AddModelStateError(ErrorMessages.AccountIsDesabled);
                }

                return this.RedirectToLogin(model.ReturnUrl);
            }

            // User has to change password (password change is required)
            if (verificationResult.NeedChangePassword)
            {
                throw new NotImplementedException(
                    "Changing passwords not implemented yet.");
            }

            // User has invalid password (handle penalty)
            if (!verificationResult.IsPasswordValid)
            {
                this.AddModelStateError(ErrorMessages.InvalidCredentials);
                return this.RedirectToLogin(model.ReturnUrl);
            }

            await this._authenticationService.SignInAsync(
                verificationResult.UserAccount,
                model.ReturnUrl,
                model.RememberLogin);

            return this.RedirectToReturnUrl(model.ReturnUrl);
        }

        [NonAction]
        internal async Task<LoginViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            LoginViewModel vm = new LoginViewModel
            {
                ReturnUrl = returnUrl,

                EnableRememberLogin = this._applicationOptions
                    .EnableRememberLogin,

                EnableAccountRegistration = this._applicationOptions
                    .EnableAccountRegistration,

                EnableAccountRecover = this._applicationOptions
                    .EnableAccountRecovery,

                // TODO: expose AuthorizationRequest
                LoginHint = this.IdentityBaseContext
                    .AuthorizationRequest.LoginHint,
            };

            /*
            // Not yet supported
            if (context?.IdP != null)
            {
                // This is meant to short circuit the UI and only trigger the one external IdP
                vm.EnableLocalLogin = applicationOptions.EnableLocalLogin;
                vm.ExternalProviders = new ExternalProvider[] {
                    new ExternalProvider { AuthenticationScheme = context.IdP }
                };

                return vm;
            }*/


            Client client = this.IdentityBaseContext.Client;

            // IEnumerable<ExternalProvider> providers = await this._clientService
            //     .GetEnabledProvidersAsync(client);

            vm.ExternalProviders = await this._authenticationService
                .GetExternalProvidersAsync();

            // TODO: remove as soon 
            vm.EnableLocalLogin = (client != null ?
                client.EnableLocalLogin : false) &&
                this._applicationOptions.EnableAccountLogin;

            // if (userAccount != null)
            // {
            //     vm.ExternalProviderHints = userAccount?.Accounts
            //         .Select(c => c.Provider);
            // }

            return vm;
        }
    }
}