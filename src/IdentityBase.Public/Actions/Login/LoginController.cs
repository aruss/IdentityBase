namespace IdentityBase.Public.Actions.Login
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    // https://github.com/IdentityServer/IdentityServer4.Samples/blob/dev/Quickstarts/5_HybridFlowAuthenticationWithApiAccess/src/QuickstartIdentityServer/Controllers/AccountController.cs
    public class LoginController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<LoginController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;

        public LoginController(
            ApplicationOptions applicationOptions,
            ILogger<LoginController> logger,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            ClientService clientService)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._clientService = clientService;
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet("login", Name = "Login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                this._logger.LogError(
                    "Login attempt with missing returnUrl parameter");

                return this.Redirect(Url.Action("Index", "Error"));
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
            // NOTE: since the theme does not renders the local login form
            // it is not possible to post this if local login is disabled
            // due to missing anti forgery token.

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
                        this.ModelState
                            .AddModelError("User account is diactivated");
                    }
                    else if (result.IsLocalAccount)
                    {
                        if (!result.IsPasswordValid)
                        {
                            this.ModelState
                                .AddModelError("Invalid credentials");

                            // TODO: Account locking on failed login attempts
                            // is not supported yet
                            // await _userAccountService
                            //     .UpdateFailedLoginAsync(result.UserAccount);
                        }
                        else
                        {
                            return await SignInAsync(model, result);
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

                this.ModelState
                    .AddModelError("Invalid username or password.");
            }

            // Something went wrong, show form with error
            return this.View(await CreateViewModelAsync(model));
        }

        [NonAction]
        internal async Task<IActionResult> SignInAsync(
            LoginInputModel model,
            UserAccountVerificationResult result)
        {
            AuthenticationProperties props = null;

            if (this._applicationOptions.EnableRememberLogin &&
                model.RememberLogin)
            {
                props = new AuthenticationProperties
                {
                    IsPersistent = true,
                    // TODO: use DateTimeAccessor
                    ExpiresUtc = DateTimeOffset.UtcNow.Add( 
                        TimeSpan.FromDays(
                            this._applicationOptions.RememberMeLoginDuration
                        )
                    )
                };
            };

            await this.HttpContext.SignInAsync(result.UserAccount, props);

            await this._userAccountService
                .UpdateSuccessfulLoginAsync(result.UserAccount);

            // Make sure the returnUrl is still valid, and if yes -
            // redirect back to authorize endpoint
            if (_interaction.IsValidReturnUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return Redirect("/");
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
                    .EnableAccountRecover,

                LoginHint = context.LoginHint,
            };

            /*
            // Not yet supported
            if (context?.IdP != null)
            {
                // This is meant to short circuit the UI and only trigger the one external IdP
                vm.EnableLocalLogin = _applicationOptions.EnableLocalLogin;
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
                this._applicationOptions.EnableLocalLogin;

            if (userAccount != null)
            {
                vm.ExternalProviderHints = userAccount?.Accounts
                    .Select(c => c.Provider);
            }

            return vm;
        }
    }
}