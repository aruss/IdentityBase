using IdentityServer4.Services;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Public.Extensions;
using IdentityBase.Services;
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
            IOptions<ApplicationOptions> applicationOptions,
            ILogger<LoginController> logger,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService,
            ClientService clientService)
        {
            _applicationOptions = applicationOptions.Value;
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
            if (ModelState.IsValid)
            {
                var result = await _userAccountService
                    .VerifyByEmailAndPasswordAsyc(model.Email, model.Password);

                if (result.UserAccount != null)
                {
                    await _userAccountService.UpdateLastUsedLocalAccountAsync(
                        result.UserAccount, result.IsPasswordValid);

                    if (!result.IsLoginAllowed)
                    {
                        ModelState.AddModelError("", "User account is diactivated");
                    }
                    else if (result.IsLocalAccount)
                    {
                        if (!result.IsPasswordValid)
                        {
                            ModelState.AddModelError("", "Invalid credentials");
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

                            // Make sure the returnUrl is still valid, and if yes -
                            // redirect back to authorize endpoint
                            if (_interaction.IsValidReturnUrl(model.ReturnUrl))
                            {
                                return Redirect(model.ReturnUrl);
                            }

                            return Redirect("~/");
                        }
                    }
                    else
                    {
                        var vm = await this.CreateViewModelAsync(model);
                        return View(vm);
                    }
                }

                ModelState.AddModelError("", "Invalid username or password.");
            }

            // Something went wrong, show form with error
            var vmx = await this.CreateViewModelAsync(model);
            return View(vmx);
        }

        public async Task<LoginViewModel> CreateViewModelAsync(string returnUrl)
        {
            return await this.CreateViewModelAsync(new LoginInputModel { ReturnUrl = returnUrl });
        }

        public async Task<LoginViewModel> CreateViewModelAsync(
            LoginInputModel inputModel,
            UserAccount userAccount = null)
        {
            var context = await _interaction.GetAuthorizationContextAsync(inputModel.ReturnUrl);
            if (context?.IdP != null)
            {
                // This is meant to short circuit the UI and only trigger the one external IdP
                return new LoginViewModel(inputModel)
                {
                    EnableLocalLogin = false,
                    Email = context?.LoginHint,
                    ExternalProviders = new ExternalProvider[] {
                        new ExternalProvider { AuthenticationScheme = context.IdP }
                    }
                };
            }

            var client = await _clientService.FindEnabledClientByIdAsync(context.ClientId);
            var providers = await _clientService.GetEnabledProvidersAsync(client);

            var vm = new LoginViewModel(inputModel)
            {
                EnableRememberLogin = _applicationOptions.EnableRememberLogin,
                Email = context?.LoginHint,
                ExternalProviders = providers.ToArray(),
                EnableLocalLogin = (client != null ? client.EnableLocalLogin : false)
                    && _applicationOptions.EnableLocalLogin
            };

            if (userAccount != null)
            {
                vm.LoginHints = userAccount?.Accounts.Select(c => c.Provider);
            }

            return vm;
        }
    }
}