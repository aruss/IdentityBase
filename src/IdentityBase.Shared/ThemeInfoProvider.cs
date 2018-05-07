// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityBase
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Extensions;
    using ServiceBase.Mvc.Theming;

    public class ThemeInfoProvider : IThemeInfoProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ThemeInfoProvider(
            IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
        }

        public Task<ThemeInfoResult> GetThemeInfoResultAsync()
        {
            HttpContext context = this._httpContextAccessor.HttpContext;

            IdentityBaseContext identityBaseContext =
                    context.GetIdentityBaseContext();

            ThemeInfoResult result = new ThemeInfoResult
            {
                RequestTheme = "DefaultTheme",
                DefaultTheme = "DefaultTheme"
            };

            if (!identityBaseContext.IsValid)
            {
                return Task.FromResult(result);
            }

            ClientProperties clientProperties = identityBaseContext.Client
                .Properties.ToObject<ClientProperties>();

            result.RequestTheme = clientProperties.Theme ?? "DefaultTheme";

            // TODO: check if plugin exists
            return Task.FromResult(result);
        }
    }
}