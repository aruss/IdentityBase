// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityBase.Web;
    using IdentityBase.Web.InputModels.Login;
    using IdentityBase.Web.ViewModels.Login;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Mvc;

    public class LoginController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;
        private readonly AuthenticationService _authenticationService;

        public LoginController(
            ILogger<LoginController> logger,
            IStringLocalizer localizer,
            ApplicationOptions applicationOptions,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            ClientService clientService,
            AuthenticationService authenticationService)
            : base(localizer, logger)
        {
            this._applicationOptions = applicationOptions;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._clientService = clientService;
            this._authenticationService = authenticationService;
        }

        /// <summary>
        /// Shows the login page.
        /// </summary>
        [HttpGet("login", Name = "Login")]
        [RestoreModelState]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel vm = await this.CreateViewModelAsync(returnUrl);

            if (vm == null)
            {
                base.Logger.LogError(
                    "Login attempt with missing returnUrl parameter");
            }

            // If local authentication is disbaled and there is only one
            // external provider provided so do just external authentication
            // without showing the login page 
            if (vm.IsExternalLoginOnly)
            {
                return this.ChallengeExternalLogin(
                    vm.ExternalProviders.First().AuthenticationScheme,
                    returnUrl);
            }

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

            // invalid input (return to same login)
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToLogin(model.ReturnUrl);
            }

            UserAccountVerificationResult result =
                await this._userAccountService.VerifyByEmailAndPasswordAsync(
                    model.Email,
                    model.Password
                );

            // user not present (wrong email)
            if (result.UserAccount == null)
            {
                // Show email or password is invalid
                return this.RedirectToLoginWithError(
                    model.ReturnUrl, ErrorMessages.InvalidCredentials);
            }

            // User may not login (deactivated)
            if (!result.IsLoginAllowed)
            {
                // If paranoya mode is on, do not epose any existence
                // of user prensentce 
                if (this._applicationOptions.ObfuscateUserAccountPresence)
                {
                    // And then send an email with info what actually happened.
                    throw new NotImplementedException(
                        "Send email with information that user account is disabled.");

                    // Show "email or password is invalid" message 
                    return this.RedirectToLoginWithError(
                        model.ReturnUrl, ErrorMessages.InvalidCredentials);
                }
                else
                {
                    return this.RedirectToLoginWithError(
                        model.ReturnUrl, ErrorMessages.AccountIsDesabled); 
                }
            }

            // User has to change password (password change is required)
            if (result.NeedChangePassword)
            {
                throw new NotImplementedException("Changing passwords not implemented yet."); 
            }

            // User has invalid password (handle penalty)
            if (!result.IsPasswordValid)
            {
                // Show "email or password is invalid" message 
                return this.RedirectToLoginWithError(
                    model.ReturnUrl, ErrorMessages.InvalidCredentials);
            }

            await this._authenticationService.SignInAsync(
                result.UserAccount,
                model.ReturnUrl,
                model.RememberLogin);

            return this.RedirectToReturnUrl(
                model.ReturnUrl,
                this._interaction);           
        }

        [NonAction]
        internal async Task<LoginViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(new LoginInputModel
            {
                ReturnUrl = returnUrl
            });
        }

        [NonAction]
        internal async Task<LoginViewModel> CreateViewModelAsync(
            LoginInputModel inputModel,
            UserAccount userAccount = null)
        {
            AuthorizationRequest context = await this._interaction
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            LoginViewModel vm = new LoginViewModel
            {
                ReturnUrl = inputModel.ReturnUrl,

                EnableRememberLogin = this._applicationOptions
                    .EnableRememberLogin,

                EnableAccountRegistration = this._applicationOptions
                    .EnableAccountRegistration,

                EnableAccountRecover = this._applicationOptions
                    .EnableAccountRecovery,

                // TODO: expose AuthorizationRequest
                LoginHint = context.LoginHint,
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

            Client client = await this._clientService
                .FindEnabledClientByIdAsync(context.ClientId);

            IEnumerable<ExternalProvider> providers = await this._clientService
                .GetEnabledProvidersAsync(client);

            vm.ExternalProviders = providers.Select(s => new
                Web.ViewModels.External.ExternalProvider
            {
                AuthenticationScheme = s.AuthenticationScheme,
                DisplayName = s.DisplayName
            }).ToArray();

            vm.EnableLocalLogin = (client != null ?
                client.EnableLocalLogin : false) &&
                this._applicationOptions.EnableAccountLogin;

            if (userAccount != null)
            {
                vm.ExternalProviderHints = userAccount?.Accounts
                    .Select(c => c.Provider);
            }

            return vm;
        }
    }
}