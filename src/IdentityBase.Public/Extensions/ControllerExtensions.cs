// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc
{
    using System.Text.Encodings.Web;
    using Microsoft.AspNetCore.Authentication;

    public static class ControllerExtensions
    {
        /// <summary>
        /// Creates an challenge action result for external login
        /// </summary>
        /// <param name="provider">
        /// Name of external provider, eg. facebook, google, hotmail
        /// </param>
        /// <returns>Instance of <see cref="ChallengeResult"/>.</returns>
        public static IActionResult ChallengeExternalLogin(
            this Controller controller,
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
    }
}