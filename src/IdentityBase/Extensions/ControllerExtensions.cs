// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc
{
    using IdentityBase.Extensions;
    using System.Text.Encodings.Web;
    using IdentityBase.Actions;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using IdentityServer4.Models;
    using System;
    using IdentityBase;

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
                "Index",
                "Login",
                new { ReturnUrl = returnUrl }
            );
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
            string returnUri,
            IIdentityServerInteractionService interactionService)
        {
            if (interactionService.IsValidReturnUrl(returnUri))
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