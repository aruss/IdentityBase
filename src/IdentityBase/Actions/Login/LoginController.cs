// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
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

    public class LoginController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<LoginController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;
        private readonly AuthenticationService _authenticationService;
        private readonly IStringLocalizer _localizer;

        public LoginController(
            ApplicationOptions applicationOptions,
            ILogger<LoginController> logger,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            ClientService clientService,          
            AuthenticationService authenticationService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._clientService = clientService;
            this._authenticationService = authenticationService;
            this._localizer = localizer;
        }

        /// <summary>
        /// Shows the login page.
        /// </summary>
        [HttpGet("login", Name = "Login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this._logger.LogError(
                    "Login attempt with missing returnUrl parameter");

                return this.RedirectToAction("Index", "Error");
            }

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
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            if (!this._applicationOptions.EnableAccountLogin)
            {
                return this.NotFound();
            }

            if (this.ModelState.IsValid)
            {
                var result = await this._userAccountService
                    .VerifyByEmailAndPasswordAsync(
                        model.Email,
                        model.Password
                    );

                if (result.UserAccount != null)
                {
                    if (!result.IsLoginAllowed)
                    {
                        this.ModelState.AddModelError(this._localizer[
                            ErrorMessages.UserAccountIsDeactivated]);
                    }
                    else if (result.IsLocalAccount)
                    {
                        if (result.IsPasswordValid)
                        {
                            await this._authenticationService.SignInAsync(
                                result.UserAccount,
                                model.ReturnUrl,
                                model.RememberLogin);

                            return this.RedirectToReturnUrl(
                                model.ReturnUrl,
                                this._interaction);
                        }
                        else
                        {
                            await this._userAccountService
                                .PerceiveFailedLoginAsync(result.UserAccount);
                        }
                    }
                    else
                    {
                        LoginViewModel vm = await this.CreateViewModelAsync(
                            model,
                            result.UserAccount
                        );

                        return this.View(vm);
                    }
                }

                this.ModelState.AddModelError(
                    this._localizer[ErrorMessages.InvalidCredentials]);
            }

            // Something went wrong, show form with error
            return this.View(await this.CreateViewModelAsync(model));
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

            LoginViewModel vm = new LoginViewModel(inputModel)
            {
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

            vm.ExternalProviders = providers.ToArray();

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