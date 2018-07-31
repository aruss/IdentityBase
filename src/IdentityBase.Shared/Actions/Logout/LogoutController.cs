// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Logout
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Mvc;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;


    public class LogoutController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;

        public LogoutController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<LogoutController> logger,
            ApplicationOptions applicationOptions,
            IEventService eventService)
        {
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this._applicationOptions = applicationOptions;
        }

        /// <summary>
        /// Show logout page at GET /logout
        /// </summary>
        [HttpGet("/logout", Name = "Logout")]
        public async Task<IActionResult> LogoutGet(LogoutInputModel model)
        {
            LogoutViewModel vm = await this
                .CreateLogoutViewModelAsync(model.LogoutId);

            if (!vm.ShowLogoutPrompt)
            {
                return await this.LogoutPost(model);
            }

            return this.View(vm);
        }

        [NonAction]
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
                // Ff the user is not authenticated, then just show logged out
                // page
                vm.ShowLogoutPrompt = false;
                return vm;
            }

            LogoutRequest context = await this.InteractionService
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
        [HttpPost("/logout", Name = "Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogoutPost(LogoutInputModel model)
        {
            // build a model so the logged out page knows what to display
            LoggedOutViewModel vm = await this
                .CreateLoggedOutViewModelAsync(model.LogoutId);

            if (User?.Identity.IsAuthenticated == true)
            {
                // Delete local authentication cookie
                await this.HttpContext.SignOutAsync();

                // TODO: Raise the logout event
                // await _events.RaiseAsync(new UserLogoutSuccessEvent(
                //      User.GetSubjectId(), User.GetDisplayName()));
            }

            // Check if we need to trigger sign-out at an upstream identity
            // provider
            if (vm.TriggerExternalSignout)
            {
                // build a return URL so the upstream provider will redirect
                // back to us after the user has logged out. this allows us to
                // then complete our single sign-out processing.
                string url = Url.Action("Logout", new
                {
                    logoutId = vm.LogoutId
                });

                // This triggers a redirect to the external provider
                // for sign-out
                return this.SignOut(new AuthenticationProperties
                {
                    RedirectUri = url
                },
                    vm.ExternalAuthenticationScheme);
            }

            return this.View("LoggedOut", vm);
        }

        [NonAction]
        private async Task<LoggedOutViewModel> CreateLoggedOutViewModelAsync(
            string logoutId)
        {
            // get context information (client name, post logout redirect URI
            /// and iframe for federated signout)
            LogoutRequest logout = await this.InteractionService
                .GetLogoutContextAsync(logoutId);

            var vm = new LoggedOutViewModel
            {
                AutomaticRedirectAfterSignOut = this._applicationOptions
                    .AutomaticRedirectAfterSignOut,

                ClientName = String.IsNullOrEmpty(logout?.ClientName) ?
                    logout?.ClientId :
                    logout?.ClientName,

                PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
                SignOutIframeUrl = logout?.SignOutIFrameUrl,
                LogoutId = logoutId
            };

            if (this.User?.Identity.IsAuthenticated == true)
            {
                string idp = this.User
                    .FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

                if (idp != null &&
                    idp != IdentityServerConstants.LocalIdentityProvider)
                {
                    bool providerSupportsSignout = await this.HttpContext
                        .GetSchemeSupportsSignOutAsync(idp);

                    if (providerSupportsSignout)
                    {
                        if (vm.LogoutId == null)
                        {
                            // if there's no current logout context, we need
                            // to create one this captures necessary info from
                            // the current logged in user before we signout and
                            // redirect away to the external IdP for signout
                            vm.LogoutId = await this.InteractionService
                                .CreateLogoutContextAsync();
                        }

                        vm.ExternalAuthenticationScheme = idp;
                    }
                }
            }

            return vm;
        }
    }
}