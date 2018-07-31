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
        public async Task<IActionResult> Challenge(
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
                RedirectUri = this.Url.Action(nameof(Callback)),
                Items = {
                    { "returnUrl", returnUrl },
                    { "scheme", provider },
                }
            };

            return this.Challenge(props, provider);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet("/external/callback", Name = "ExternalCallback")]
        public async Task<IActionResult> Callback()
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

            // Lookup our user and external provider info
            (
                UserAccount userAccount,
                string provider,
                string subject,
                string email,
                IEnumerable<Claim> claims
            ) = await this.FindUserFromExternalProviderAsync(authResult);

            if (userAccount != null)
            {
                // Update last authenticated flags

                // Authenticate and redirect to return url
                return await this.SignInAsync(
                    userAccount,
                    authResult,
                    provider);
            }

            userAccount = await this._userAccountStore
               .LoadByEmailAsync(email);

            if (userAccount == null)
            {
                string returnUrl = authResult.Properties.Items["returnUrl"];

                if (String.IsNullOrWhiteSpace(returnUrl))
                {
                    throw new Exception("External authentication error");
                }

                DateTime now = DateTime.UtcNow;
                Guid userAccountId = Guid.NewGuid();

                userAccount = new UserAccount
                {
                    Id = userAccountId,
                    Email = email,
                    FailedLoginCount = 0,
                    IsEmailVerified = false,
                    IsLoginAllowed = this._applicationOptions
                        .RequireLocalAccountVerification,
                    Accounts = new ExternalAccount[]
                    {
                        new ExternalAccount
                        {
                            Email = email,
                            UserAccountId = userAccountId,
                            Provider = provider,
                            Subject = subject,
                            LastLoginAt = null,
                            IsLoginAllowed = this._applicationOptions
                                .RequireExternalAccountVerification
                        }
                    }
                };

                if (this._applicationOptions
                    .RequireExternalAccountVerification)
                {
                    this._userAccountService.SetVerificationData(
                        userAccount,
                        VerificationKeyPurpose.ConfirmAccount,
                        returnUrl,
                        now
                    );

                    await this._userAccountStore.WriteAsync(userAccount);

                    await this._notificationService
                        .SendUserAccountCreatedEmailAsync(userAccount);

                    // TODO: Emit user created event

                    // TODO: Create provider via some helper
                    return this.View("Success", new SuccessViewModel
                    {
                        ReturnUrl = returnUrl,
                        Provider = userAccount.Email
                            .Split('@')
                            .LastOrDefault()
                    });
                }
                else
                {
                    // Update last authenticated flags
                    userAccount.LastLoginAt = now;

                    await this._userAccountStore.WriteAsync(userAccount);

                    // TODO: Emit user created event

                    // Authenticate and redirect to return url
                    return await this.SignInAsync(
                        userAccount,
                        authResult,
                        provider);
                }
            }
            else
            {
                // Merge accounts
                if (this._applicationOptions.AutomaticAccountMerge)
                {
                    // Merge accounts automaticly without asking the user

                    DateTime now = DateTime.UtcNow;
                    var externalAccount = new ExternalAccount
                    {
                        Email = email,
                        UserAccountId = userAccount.Id,
                        Provider = provider,
                        Subject = subject,
                        LastLoginAt = now,
                        IsLoginAllowed = true
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

                    await this._userAccountStore.WriteAsync(userAccount);

                    // TODO: Emit user account updated event

                    return await this.SignInAsync(
                      userAccount,
                      authResult,
                      provider);
                }
                else
                {
                    // Ask user if he wants to merge accounts or use another
                    // account.
                    throw new NotImplementedException();
                }
            }
        }

        private async Task<IActionResult> SignInAsync(
            UserAccount userAccount,
            AuthenticateResult authResult,
            string provider)
        {
            // retrieve return URL
            string returnUrl = authResult.Properties.Items["returnUrl"];

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
                        Name = "id_token",
                        Value = idToken
                    }
                });
            }
        }
    }
}