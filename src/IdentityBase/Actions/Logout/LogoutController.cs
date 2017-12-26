// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Logout
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    public class LogoutController : WebController
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(
            IIdentityServerInteractionService interaction,
            ApplicationOptions applicationOptions,
            IEventService eventService,
            ILogger<LogoutController> logger)
        {
            this._interaction = interaction;
            this._applicationOptions = applicationOptions;
            this._logger = logger;
        }

        /// <summary>
        /// Show logout page at GET /logout
        /// </summary>
        [HttpGet("logout", Name = "Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            LogoutViewModel vm = await this
                .CreateLogoutViewModelAsync(logoutId);

            if (!vm.ShowLogoutPrompt)
            {
                return await this.Logout(vm);
            }

            return this.View(vm);
        }

        private async Task<LogoutViewModel> CreateLogoutViewModelAsync(
            string logoutId)
        {
            LogoutViewModel vm = new LogoutViewModel
            {
                LogoutId = logoutId,
                ShowLogoutPrompt = this._applicationOptions.ShowLogoutPrompt
            };

            ClaimsPrincipal user = this.HttpContext.User;
            if (user == null || user.Identity.IsAuthenticated == false)
            {
                // Ff the user is not authenticated, then just show logged out page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            LogoutRequest context = await this._interaction
                .GetLogoutContextAsync(logoutId);

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
        /// Handle logout page postback at POST /logout
        /// </summary>
        [HttpPost("logout", Name = "LoggedOut")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(LogoutViewModel model)
        {
            LoggedOutViewModel vm = await this
                .CreateLoggedOutViewModelAsync(model.LogoutId);

            ClaimsPrincipal user = HttpContext.User;
            if (user?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();

                // raise the logout event
                // await events.RaiseAsync(new UserLogoutSuccessEvent(user.GetSubjectId(), user.GetName()));
            }

            // check if we need to trigger sign-out at an upstream identity provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect back
                // to us after the user has logged out. this allows us to then
                // complete our single sign-out processing.
                string url = this.Url.Action(
                    "Logout",
                    new { logoutId = vm.LogoutId }
                );

                // Hack: try/catch to handle social providers that throw
                try
                {
                    // this triggers a redirect to the external provider for sign-out
                    return this.SignOut(
                        new AuthenticationProperties { RedirectUri = url },
                        vm.ExternalAuthenticationScheme);
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

            return this.View("LoggedOut", vm);
        }

        private async Task<LoggedOutViewModel> CreateLoggedOutViewModelAsync(
            string logoutId)
        {
            // get context information (client name, post logout redirect URI and iframe for federated signout)
            LogoutRequest logout = await this._interaction
                .GetLogoutContextAsync(logoutId);

            LoggedOutViewModel vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = this._applicationOptions
                    .AutomaticRedirectAfterSignOut,

                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                ClientName = logout?.ClientId,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            ClaimsPrincipal user = HttpContext.User;
            if (user != null)
            {
                string idp = user.FindFirst(
                    JwtClaimTypes.IdentityProvider)?.Value;

                if (idp != null && idp != IdentityServerConstants
                    .LocalIdentityProvider)
                {
                    if (vm.LogoutId == null)
                    {
                        // if there's no current logout context, we need to create one
                        // this captures necessary info from the current logged in user
                        // before we signout and redirect away to the external IdP for signout
                        vm.LogoutId = await this._interaction
                            .CreateLogoutContextAsync();
                    }

                    vm.ExternalAuthenticationScheme = idp;
                }
            }

            return vm;
        }
    }
}