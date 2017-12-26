// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using IdentityServer4.Extensions;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Extensions;

    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the public base URL for Identity Base.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetBaseUrl(this HttpContext httpContext)
        {
            return httpContext
                .GetIdentityServerBaseUrl()
                .EnsureTrailingSlash();
        }
    }
}
