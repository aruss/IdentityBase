// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.External
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityModel;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Extensions;
    using IdBaseAuthService = IdentityBase.Services.AuthenticationService;

    public class ExternalController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly IUserAccountStore _userAccountStore;
        private readonly IdBaseAuthService _authenticationService;
        private readonly NotificationService _notificationService;
        private readonly UserAccountService _userAccountService;

        public ExternalController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<ExternalController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            IUserAccountStore userAccountStore,
            IdBaseAuthService authenticationService,
            NotificationService notificationService,
            UserAccountService userAccountService)
        {
            // setting it this way since interaction service is null in the
            // base class oO
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._userAccountStore = userAccountStore;
            this._authenticationService = authenticationService;
            this._notificationService = notificationService;
            this._userAccountService = userAccountService;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet("/external", Name = "External")]
        public async Task<IActionResult> ChallengeGet(
            string provider,
            string returnUrl)
        {
            // if (AccountOptions.WindowsAuthenticationSchemeName == provider)
            // {
            //     // windows authentication needs special handling
            //     return await ProcessWindowsLoginAsync(returnUrl);
            // }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = this.Url.Action(nameof(CallbackGet)),
                Items = {
                    {"returnUrl", returnUrl },
                    {"scheme", provider },
                }
            };

            return this.Challenge(props, provider);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet("/external/callback", Name = "ExternalCallback")]
        public async Task<IActionResult> CallbackGet()
        {
            // Read external identity from the temporary cookie
            AuthenticateResult authResult =
                await this.HttpContext.AuthenticateAsync(
                    IdentityServer4.IdentityServerConstants.
                    ExternalCookieAuthenticationScheme);

            if (authResult?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            string returnUrl = authResult.Properties.Items["returnUrl"];

            // Lookup our user and external provider info
            (
                UserAccount externalAccount,
                string provider,
                string subject,
                string email,
                IEnumerable<Claim> claims
            ) = await this.FindUserFromExternalProviderAsync(authResult);

            UserAccount userAccount =
                await this._authenticationService
                    .GetAuthenticatedUserAccountAsync() ??
                externalAccount ??
                await this._userAccountStore.LoadByEmailAsync(email);

            bool isNewUserAccount = userAccount == null;

            if (userAccount != null)
            {
                if (externalAccount == null)
                {
                    if (this._applicationOptions.AutomaticAccountMerge)
                    {
                        this.Logger.LogDebug(
                            "Create and merge external account with existing account"
                        );

                        userAccount = this.CreateUserAccount(
                            provider,
                            subject,
                            email,
                            claims,
                            userAccount);
                    }
                    else
                    {
                        // Redirect here to action where the user will be
                        // asked if he wants to proceed and merge accounts
                        // if user says yes then call CreateNewAndMergeWithExisting
                        throw new NotImplementedException();
                    }
                }
                else if (!externalAccount.Id.Equals(userAccount.Id))
                {
                    this.Logger.LogDebug(
                        "External account already merged with different existing account"
                    );

                    return this.RedirectToRoute("Error", new ErrorViewModel
                    {
                        ReturnUrl = returnUrl
                    });
                }
            }
            else
            {
                this.Logger.LogDebug(
                    "Create a new account from external account"
                );

                userAccount = this.CreateUserAccount(
                    provider,
                    subject,
                    email,
                    claims);
            }

            IActionResult result = null;

            if (!this.HttpContext.User.Identity.IsAuthenticated &&
                this.InteractionService.IsValidReturnUrl(returnUrl))
            {
                this.Logger.LogDebug(
                    "Updating users last login information");

                this.UpdateUserAccountLastLoginInfo(userAccount, provider);

                this.Logger.LogDebug(
                   "Signing in user account and redirecting to return URL");

                result = await this.SignInAsync(
                       userAccount,
                       authResult,
                       provider,
                       returnUrl);
            }
            else
            {
                this.Logger.LogDebug(
                   "Redirecting to local return URL");

                result = this.LocalRedirect(returnUrl);
            }

            await this._userAccountStore.WriteAsync(userAccount);

            if (isNewUserAccount)
            {
                // TODO: emit user created event
            }
            else
            {
                // TODO: emit user updated event
            }

            if (this.HttpContext.User.Identity.IsAuthenticated)
            {
                // TODO: emit user authenticated event
            }

            return result;
        }

        [NonAction]
        private void UpdateUserAccountLastLoginInfo(
            UserAccount userAccount,
            string provider)
        {
            ExternalAccount account = userAccount.Accounts
                .FirstOrDefault(c => c.Provider.Equals(
                        provider,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                );

            DateTime now = DateTime.UtcNow;

            account.LastLoginAt = now;
        }

        [NonAction]
        private UserAccount CreateUserAccount(
            string provider,
            string subject,
            string email,
            IEnumerable<Claim> claims,
            UserAccount userAccount = null)
        {
            DateTime now = DateTime.UtcNow;
            Guid userAccountId = Guid.NewGuid();

            if (userAccount == null)
            {
                userAccount = new UserAccount
                {
                    Id = userAccountId,
                    Email = email,
                    FailedLoginCount = 0,
                    IsEmailVerified = false,
                    IsActive = true
                };
            }

            var externalAccount = new ExternalAccount
            {
                Email = email,
                UserAccountId = userAccount.Id,
                Provider = provider,
                Subject = subject,
                LastLoginAt = now
            };

            if (userAccount.Accounts != null)
            {
                (userAccount.Accounts as List<ExternalAccount>)
                    .Add(externalAccount);
            }
            else
            {
                userAccount.Accounts = new ExternalAccount[]
                {
                    externalAccount
                };
            }

            return userAccount;
        }

        [NonAction]
        private async Task<IActionResult> SignInAsync(
            UserAccount userAccount,
            AuthenticateResult authResult,
            string provider,
            string returnUrl)
        {
            if (String.IsNullOrWhiteSpace(returnUrl))
            {
                throw new Exception("External authentication error");
            }

            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local
            // auth cookie. This is typically used to store data needed for
            // signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();

            this.ProcessLoginCallbackForOidc(
                authResult, additionalLocalClaims, localSignInProps);

            await HttpContext.SignInAsync(
                userAccount.Id.ToString(),
                userAccount.Email,
                provider,
                localSignInProps,
                additionalLocalClaims.ToArray());

            // Delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(
                IdentityServer4.IdentityServerConstants
                    .ExternalCookieAuthenticationScheme);

            // check if external login is in the context of an OIDC request
            AuthorizationRequest context = await this.InteractionService
               .GetAuthorizationContextAsync(returnUrl);

            // TODO: Check if additional information is required, if yes redirect to form 

            // if (context != null)
            // {
            //     Client client = await _clientStore
            //         .FindEnabledClientByIdAsync(context.ClientId);
            // 
            //     if (client?.RequirePkce == true)
            //     {
            //         // if the client is PKCE then we assume it's native, so
            //         // this change in how to return the response is for better
            //         // UX for the end user.
            //         return this.View("Redirect", new RedirectViewModel
            //         {
            //             RedirectUrl = returnUrl
            //         });
            //     }
            // 
            // }

            // TODO: Emit user authenticated event
            // TODO: Validate return url 

            return this.Redirect(returnUrl);
        }

        [NonAction]
        private async Task<(
            UserAccount userAccount,
            string provider,
            string subject,
            string email,
            IEnumerable<Claim> claims)>
            FindUserFromExternalProviderAsync(AuthenticateResult result)
        {
            ClaimsPrincipal principal = result.Principal;

            // try to determine the unique id of the external user (issued by
            // the provider) the most common claim type for that are the sub
            // claim and the NameIdentifier depending on the external provider,
            // some other claim type might be used
            Claim subjectClaim = principal.FindFirst(JwtClaimTypes.Subject) ??
                              principal.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            if (subjectClaim == null)
            {
                throw new Exception("External authentication error");
            }

            // remove the user id claim so we don't include it as an extra
            // claim if/when we provision the user
            List<Claim> claims = principal.Claims.ToList();
            //claims.Remove(subjectClaim);

            string provider = result.Properties.Items["scheme"];
            string subject = subjectClaim.Value;

            Claim emailClaim = claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Email);

            string email = emailClaim.Value;

            if (emailClaim == null)
            {
                throw new Exception("External authentication error");
            }

            // Find external user
            UserAccount userAccount = await this._userAccountStore
                .LoadByExternalProviderAsync(provider, subject);

            return (userAccount, provider, subject, email, claims);
        }

        private void ProcessLoginCallbackForOidc(
            AuthenticateResult externalResult,
            List<Claim> localClaims,
            AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            Claim sid = externalResult.Principal.Claims
                .FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);

            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // If the external provider issued an id_token, we'll keep it for
            // signout
            string idToken = externalResult.Properties
                .GetTokenValue("id_token");

            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] {
                    new AuthenticationToken {
                        Name ="id_token",
                        Value = idToken
                    }
                });
            }
        }
    }
}