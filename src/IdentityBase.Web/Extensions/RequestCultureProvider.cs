// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    public class RequestCultureProvider : IRequestCultureProvider
    {
        public Task<ProviderCultureResult>
            DetermineProviderCultureResult(HttpContext httpContext)
        {
            IdentityBaseContext idbContext =
                httpContext.RequestServices
                    .GetService<IdentityBaseContext>();

            ApplicationOptions appOptions =
                httpContext.RequestServices
                    .GetService<ApplicationOptions>();

            string culture = null;

            // Try get culture from authorization request 
            if (idbContext.IsValid)
            {
                // If query param is empty but authorization request is present
                // check inside authorization request 
                if (String.IsNullOrWhiteSpace(culture) &&
                    idbContext.AuthorizationRequest != null)
                {
                    culture = idbContext
                        .AuthorizationRequest
                        .Parameters?
                        .GetValues("culture")?
                        .FirstOrDefault();
                }
            }

            // Try get culture from query string
            if (String.IsNullOrWhiteSpace(culture))
            {
                culture = httpContext.Request.Query["culture"];
            }
            // Try get from client properties 
            else if (idbContext.ClientProperties != null)
            {
                culture = idbContext.ClientProperties.Culture;
            }

            // Try get from cookie
            string cookieValue = httpContext.Request.Cookies["idb.c"];

            if (String.IsNullOrWhiteSpace(culture))
            {
                culture = cookieValue;
            }

            // Set default
            if (String.IsNullOrWhiteSpace(culture))
            {
                culture = appOptions.DefaultCulture;
            }

            // Set cookie
            if (!culture.Equals(cookieValue))
            {
                CookieOptions option = new CookieOptions
                {
                    HttpOnly = true
                };

                httpContext.Response.Cookies.Append("idb.c", culture, option);
            }

            return Task.FromResult(
                new ProviderCultureResult(new StringSegment(culture)));
        }
    }
}
