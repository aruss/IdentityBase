// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Mvc
{
    using System;
    using System.Security.Policy;
    using IdentityBase;
    using IdentityBase.Actions.External;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// <see cref="WebController"/> extension methods.
    /// </summary>
    public static partial class WebControllerExtensions
    {
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
            return controller.RedirectToRoute(
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