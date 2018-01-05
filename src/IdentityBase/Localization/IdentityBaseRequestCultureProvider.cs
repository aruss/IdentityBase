// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Localization
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    public class IdentityBaseRequestCultureProvider : IRequestCultureProvider
    {
        public async Task<ProviderCultureResult>
            DetermineProviderCultureResult(HttpContext httpContext)
        {
            IdentityBaseContext identityBaseContext =
                httpContext.RequestServices
                    .GetService<IdentityBaseContext>();

            if (identityBaseContext.IsValid)
            {
                string value = identityBaseContext
                    .AuthorizationRequest
                    .Parameters?
                    .GetValues("culture")?
                    .FirstOrDefault();

                if (value != null)
                {
                    return new ProviderCultureResult(
                        new StringSegment(value));
                }
            }

            return null;
        }
    }
}
