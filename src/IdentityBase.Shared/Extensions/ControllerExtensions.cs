// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc
{
    using System;
    using System.Text.Encodings.Web;
    using IdentityBase;
    using IdentityBase.Extensions;
    using IdentityBase.Web;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;

    /// <summary>
    /// <see cref="WebController"/> extension methods.
    /// </summary>
    public static partial class WebControllerExtensions
    {
        /// <summary>
        /// Creates an challenge action result for external login
        /// </summary>
        /// <param name="provider">
        /// Name of external provider, eg. facebook, google, hotmail
        /// </param>
        /// <returns>Instance of <see cref="ChallengeResult"/>.</returns>
        public static IActionResult ChallengeExternalLogin(
            this WebController controller,
            string provider,
            string returnUrl)
        {
            if (returnUrl != null)
            {
                returnUrl = UrlEncoder.Default.Encode(returnUrl);
            }

            returnUrl = "external-callback?returnUrl=" + returnUrl;

            // Start challenge and roundtrip the return URL
            AuthenticationProperties props = new AuthenticationProperties
            {
                RedirectUri = returnUrl,
                Items = { { "scheme", provider } }
            };

            return new ChallengeResult(provider, props);
        }

        /// <summary>
        /// Redirects to login page.
        /// </summary>
        /// <param name="controller">The instance of
        /// <see cref="WebController"/>.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <returns>The created <see cref="RedirectResult"/> for
        /// the response.</returns>
        public static IActionResult RedirectToLogin(
            this WebController controller,
            string returnUrl)
        {
            return controller.RedirectToAction(
                "Login",
                "Login",
                new { ReturnUrl = returnUrl }
            );
        }

        public static void AddModelStateError(
            this WebController controller,
            string key,
            string errorMessage)
        {
            controller.ModelState.AddModelError(
                key,
                controller.Localizer[errorMessage]);
        }

        public static void AddModelStateError(
            this WebController controller,
            string errorMessage)
        {
            controller.ModelState.AddModelError(
                controller.Localizer[errorMessage]);
        }

        /// <summary>
        /// If <paramref name="returnUrl"/> is valid it will redirect to it,
        /// otherwise will redirect to landing page
        /// </summary>
        /// <param name="controller">The instance of
        /// <see cref="WebController"/>.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="interactionService">The instance of
        /// <see cref="IIdentityServerInteractionService"/> used to
        /// validate returnUrl.</param>
        /// <returns>The created <see cref="RedirectResult"/> for
        /// the response.</returns>
        public static IActionResult RedirectToReturnUrl(
            this WebController controller,
            string returnUri)
        {
            if (controller.InteractionService.IsValidReturnUrl(returnUri))
            {
                return controller.Redirect(returnUri);
            }

            IdentityBaseContext idbContext =
                controller.HttpContext.GetIdentityBaseContext();

            if (idbContext?.Client != null)
            {
                returnUri = idbContext.Client.TryGetReturnUri(returnUri);
            }

            if (String.IsNullOrWhiteSpace(returnUri))
            {
                throw new ApplicationException("Invalid returnUri");
            }

            return controller.Redirect(returnUri);
        }
    }
}