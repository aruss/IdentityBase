// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
{
    using System;
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
    using ServiceBase.Mvc;

    public class LoginController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly IUserAccountStore _userAccountStore;
        private readonly AuthenticationService _authenticationService;
        private readonly UserAccountService _userAccountService;

        public LoginController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<LoginController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            IUserAccountStore userAccountStore,
            AuthenticationService authenticationService,
            UserAccountService userAccountService)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._userAccountStore = userAccountStore;
            this._authenticationService = authenticationService;
            this._userAccountService = userAccountService;
        }

        /// <summary>
        /// Shows the login page with local and external logins.
        /// </summary>
        [HttpGet("/login", Name = "Login")]
        [RestoreModelState]
        public async Task<IActionResult> LoginGet(string returnUrl)
        {
            LoginViewModel vm = await this.CreateViewModelAsync(returnUrl);

            // If local authentication is disbaled and there is only one
            // external provider provided so do just external authentication
            // without showing the login page 
            if (vm.IsExternalLoginOnly)
            {
                return this.RedirectToRoute(
                    "External",
                    new
                    {
                        provider = vm.ExternalProviders
                            .First().AuthenticationScheme,

                        returnUrl = returnUrl
                    }
                );
            }

            vm.FormModel = await this.CreateFormViewModelAsync
                <ILoginCreateViewModelAction>(vm);

            return this.View("Login", vm);
        }

        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost("/login", Name = "Login")]
        [ValidateAntiForgeryToken]
        [StoreModelState]
        public async Task<IActionResult> LoginPost(LoginInputModel model)
        {
            // TODO: extract in own controller and not add it
            if (!this._applicationOptions.EnableAccountLogin)
            {
                return this.NotFound();
            }

            BindInputModelResult formResult = await this
                .BindFormInputModelAsync<ILoginBindInputModelAction>();

            // invalid input (return to same login)
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToLogin(model.ReturnUrl);
            }

            (
                UserAccount userAccount,
                bool isLocalAccount,
                bool isAccountActive,
                bool isPasswordValid,
                bool needChangePassword,
                bool needEmailVerification,
                string[] hints
            ) = await this.VerifyByEmailAndPasswordAsync(
                model.Email,
                model.Password);

            // user not present (wrong email)
            if (userAccount == null)
            {
                // Show email or password is invalid
                this.AddModelStateError(ErrorMessages.InvalidCredentials);
                return this.RedirectToLogin(model.ReturnUrl);
            }

            // User is not active
            if (!isAccountActive)
            {
                this.AddModelStateError(
                    nameof(LoginViewModel.Email),
                    ErrorMessages.UserAccountIsInactive);

                return this.RedirectToLogin(model.ReturnUrl);
            }

            if (needEmailVerification)
            {
                this.AddModelStateError(
                   nameof(LoginViewModel.Email),
                   ErrorMessages.UserAccountNeedsConfirmation);

                return this.RedirectToLogin(model.ReturnUrl);
            }

            // User has to change password (password change is required)
            if (needChangePassword)
            {
                throw new NotImplementedException(
                    "Changing passwords not implemented yet.");
            }

            // User has invalid password (handle penalty)
            if (!isPasswordValid)
            {
                this.AddModelStateError(ErrorMessages.InvalidCredentials);

                this._userAccountService.SetFailedSignIn(userAccount);
                await this._userAccountStore.WriteAsync(userAccount);

                // TODO: emit user updated event

                return this.RedirectToLogin(model.ReturnUrl);
            }

            await this._authenticationService.SignInAsync(
                userAccount,
                model.ReturnUrl,
                model.RememberLogin);

            this._userAccountService.SetSuccessfullSignIn(userAccount);
            await this._userAccountStore.WriteAsync(userAccount);

            // TODO: emit user updated event
            // TODO: emit user authenticated event
            // TODO: check if 2factor auth is enabled 

            return this.RedirectToReturnUrl(model.ReturnUrl);
        }

        [NonAction]
        private async Task<LoginViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            LoginViewModel vm = new LoginViewModel
            {
                ReturnUrl = returnUrl,

                EnableRememberMe = this._applicationOptions
                    .EnableRememberMe,

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

        [NonAction]
        private async Task<(
            UserAccount userAccount,
            bool isLocalAccount,
            bool isAccountActive,
            bool isPasswordValid,
            bool needChangePassword,
            bool needEmailVerification,
            string[] hints
        )>
        VerifyByEmailAndPasswordAsync(
            string email,
            string password
        )
        {
            UserAccount userAccount = await this._userAccountStore
               .LoadByEmailAsync(email);

            bool isLocalAccount = false;
            bool isAccountActive = false;
            bool isPasswordValid = false;
            bool needChangePassword = false;
            bool needEmailVerification = false;
            string[] hints = null;

            // No ueser, nothing to do here 
            if (userAccount == null)
            {
                return (
                    userAccount,
                    isLocalAccount,
                    isAccountActive,
                    isPasswordValid,
                    needChangePassword,
                    needEmailVerification,
                    hints
                );
            }

            // If user has a password so it has a local account credentials
            if (userAccount.HasPassword())
            {
                // It has a local account, so dont ask the user to create a
                // local account 
                isLocalAccount = true;

                // Check if password is valid
                isPasswordValid = this._userAccountService.IsPasswordValid(
                    userAccount.PasswordHash, password);

                // TODO: implement invalid passowrd policy 
            }

            // User is disabled 
            isAccountActive = userAccount.IsActive;

            // User requred to verify his email address
            needEmailVerification = !userAccount.IsEmailVerified &&
                this._applicationOptions.RequireLocalAccountVerification;

            // TODO: Implement password invalidation policy
            needChangePassword = false;

            // TODO: implement hints if user should get help to get authenticated

            return (
                userAccount,
                isLocalAccount,
                isAccountActive,
                isPasswordValid,
                needChangePassword,
                needEmailVerification,
                hints
            );
        }
    }
}