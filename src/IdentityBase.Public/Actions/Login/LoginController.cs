using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public.Actions.Login
{
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
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _userAccountService = userAccountService;
            _clientService = clientService;
        }

        /// <summary>
        /// Show login page
        /// </summary>
        [HttpGet("login", Name = "Login")]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                _logger.LogError("Login attempt with missing returnUrl parameter");
                return Redirect(Url.Action("Index", "Error"));
            }

            if (vm.IsExternalLoginOnly)
            {
                return this.ChallengeExternalLogin(
                    vm.ExternalProviders.First().AuthenticationScheme,
                    returnUrl);
            }

            return View(vm);
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

            if (ModelState.IsValid)
            {
                var result = await _userAccountService
                    .VerifyByEmailAndPasswordAsync(model.Email, model.Password);

                if (result.UserAccount != null)
                {
                    if (!result.IsLoginAllowed)
                    {
                        ModelState.AddModelError("User account is diactivated");
                    }
                    else if (result.IsLocalAccount)
                    {
                        if (!result.IsPasswordValid)
                        {
                            ModelState.AddModelError("Invalid credentials");

                            // TODO: Account locking on failed login attempts is not supported yet
                            //await _userAccountService.UpdateFailedLoginAsync(result.UserAccount);
                        }
                        else
                        {
                            AuthenticationProperties props = null;

                            if (_applicationOptions.EnableRememberLogin && model.RememberLogin)
                            {
                                props = new AuthenticationProperties
                                {
                                    IsPersistent = true,
                                    ExpiresUtc = DateTimeOffset.UtcNow.Add(
                                        TimeSpan.FromDays(_applicationOptions.RememberMeLoginDuration))
                                };
                            };

                            await HttpContext.Authentication.SignInAsync(result.UserAccount, props);
                            await _userAccountService.UpdateSuccessfulLoginAsync(result.UserAccount);

                            // Make sure the returnUrl is still valid, and if yes -
                            // redirect back to authorize endpoint
                            if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                            {
                                return Redirect(model.ReturnUrl);
                            }

                            return Redirect("/");
                        }
                    }
                    else
                    {
                        return View(await CreateViewModelAsync(model, result.UserAccount));
                    }
                }

                ModelState.AddModelError("Invalid username or password.");
            }

            // Something went wrong, show form with error
            return View(await CreateViewModelAsync(model));
        }

        [NonAction]
        internal async Task<LoginViewModel> CreateViewModelAsync(string returnUrl)
        {
            return await this.CreateViewModelAsync(new LoginInputModel { ReturnUrl = returnUrl });
        }

        [NonAction]
        internal async Task<LoginViewModel> CreateViewModelAsync(
            LoginInputModel inputModel,
            UserAccount userAccount = null)
        {
            var context = await _interaction.GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            var vm = new LoginViewModel(inputModel)
            {
                EnableRememberLogin = _applicationOptions.EnableRememberLogin,
                EnableAccountRegistration = _applicationOptions.EnableAccountRegistration,
                EnableAccountRecover = _applicationOptions.EnableAccountRecover,
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

            var client = await _clientService.FindEnabledClientByIdAsync(context.ClientId);
            var providers = await _clientService.GetEnabledProvidersAsync(client);

            vm.ExternalProviders = providers.ToArray();
            vm.EnableLocalLogin = (client != null ? client.EnableLocalLogin : false)
                && _applicationOptions.EnableLocalLogin;

            if (userAccount != null)
            {
                vm.ExternalProviderHints = userAccount?.Accounts.Select(c => c.Provider);
            }

            return vm;
        }
    }
}