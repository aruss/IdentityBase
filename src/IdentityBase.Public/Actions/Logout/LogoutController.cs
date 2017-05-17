using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IdentityBase.Configuration;
using System;
using System.Threading.Tasks;

namespace IdentityBase.Public.Actions.Logout
{
    public class LogoutController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(
            IIdentityServerInteractionService interaction,
            ApplicationOptions applicationOptions,
            ILogger<LogoutController> logger)
        {
            _interaction = interaction;
            _applicationOptions = applicationOptions;
            _logger = logger;
        }

        /// <summary>
        /// Show logout page
        /// </summary>
        [HttpGet("logout", Name = "Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var vm = await this.CreateLogoutViewModelAsync(logoutId);
            if (!vm.ShowLogoutPrompt)
            {
                // no need to show prompt
                return await Logout(vm);
            }

            return View(vm);
        }

        private async Task<LogoutViewModel> CreateLogoutViewModelAsync(string logoutId)
        {
            var vm = new LogoutViewModel
            {
                LogoutId = logoutId,
                ShowLogoutPrompt = _applicationOptions.ShowLogoutPrompt
            };

            var user = await this.HttpContext.GetIdentityServerUserAsync();
            if (user == null || user.Identity.IsAuthenticated == false)
            {
                // Ff the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            // Show the logout prompt. this prevents attacks where the user
            // is automatically signed out by another malicious web page.
            return vm;
        }

        /// <summary>
        /// Handle logout page postback
        /// </summary>
        [HttpPost("logout", Name = "LoggedOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutViewModel model)
        {
            var vm = await this.CreateLoggedOutViewModelAsync(model.LogoutId);
            if (vm.TriggerExternalSignout)
            {
                var url = Url.Action("Logout", new { logoutId = vm.LogoutId });
                try
                {
                    // Hack: try/catch to handle social providers that throw
                    await HttpContext.Authentication.SignOutAsync(vm.ExternalAuthenticationScheme,
                        new AuthenticationProperties { RedirectUri = url });
                }
                // This is for the external providers that don't have signout
                catch (NotSupportedException)
                {
                }
                // This is for Windows/Negotiate
                catch (InvalidOperationException)
                {
                }
            }

            // delete local authentication cookie
            await HttpContext.Authentication.SignOutAsync();

            return View("LoggedOut", vm);
        }

        private async Task<LoggedOutViewModel> CreateLoggedOutViewModelAsync(string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            var logout = await _interaction.GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = _applicationOptions.AutomaticRedirectAfterSignOut,
                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = logout?.ClientId,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            var user = await HttpContext.GetIdentityServerUserAsync();
            if (user != null)
            {
                var idp = user.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
                if (idp != null && idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    if (vm.LogoutId == null)
                    {
                        // if there's no current logout context, we need to create one
                        // this captures necessary info from the current logged in user
                        // before we signout and redirect away to the external IdP for signout
                        vm.LogoutId = await _interaction.CreateLogoutContextAsync();
                    }

                    vm.ExternalAuthenticationScheme = idp;
                }
            }

            return vm;
        }
    }
}