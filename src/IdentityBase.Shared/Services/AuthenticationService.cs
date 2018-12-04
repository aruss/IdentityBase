// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using ServiceBase;

    public class AuthenticationService
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IUserAccountStore _userAccountStore;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationOptions _applicationOptions;
        private readonly IDateTimeAccessor _dateTimeAccessor;
        private readonly IAuthenticationSchemeProvider _schemeProvider;

        public AuthenticationService(
            IIdentityServerInteractionService interaction,
            IUserAccountStore userAccountStore,
            IHttpContextAccessor httpContextAccessor,
            ApplicationOptions applicationOptions,
            IDateTimeAccessor dateTimeAccessor,
            IAuthenticationSchemeProvider schemeProvider)
        {
            this._interaction = interaction;
            this._userAccountStore = userAccountStore;
            this._httpContextAccessor = httpContextAccessor;
            this._applicationOptions = applicationOptions;
            this._dateTimeAccessor = dateTimeAccessor;
            this._schemeProvider = schemeProvider;
        }

        /// <summary>
        /// Signs the <see cref="UserAccount"/> in.
        /// </summary>
        /// <param name="userAccount">The instance of
        /// <see cref="UserAccount"/>.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="properties">The properties.</param>
        public async Task SignInAsync(
           UserAccount userAccount,
           string returnUrl,
           bool rememberLogin = false)
        {
            AuthenticationProperties properties = null;

            if (this._applicationOptions.EnableRememberMe && rememberLogin)
            {
                properties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = this._dateTimeAccessor.UtcNow.Add(
                        TimeSpan.FromSeconds(
                            this._applicationOptions.RememberMeDuration
                        )
                    )
                };
            };

            await this._httpContextAccessor.HttpContext.SignInAsync(
                userAccount.Id.ToString(),
                userAccount.Email,
                properties);
        }

        public async Task<UserAccount> GetAuthenticatedUserAccountAsync()
        {
            ClaimsPrincipal user = this._httpContextAccessor.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                return null;
            }

            Claim subjectClaim = user.FindFirst("sub");

            if (subjectClaim == null)
            {
                throw new ApplicationException(
                    "Authenticated user does not have a sub claim"
                );
            }

            Guid userId = Guid.Parse(subjectClaim.Value);

            UserAccount userAccount = await this._userAccountStore
                .LoadByIdAsync(userId);

            return userAccount;
        }

        public async Task<IEnumerable<ExternalProvider>>
            GetExternalProvidersAsync()
        {
            IEnumerable<AuthenticationScheme> schemes =
                await this._schemeProvider.GetAllSchemesAsync();

            IEnumerable<ExternalProvider> providers = schemes
                .Where(x => x.DisplayName != null
                /* || (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))*/
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            // TODO: filter providers by current active client

            return providers.ToArray();
        }
    }
}
