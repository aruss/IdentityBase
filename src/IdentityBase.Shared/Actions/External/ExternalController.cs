namespace IdentityBase.Actions.External
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityBase.Actions.Shared;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using IdentityBase.Mvc;
    using IdentityBase.Services;
    using IdentityModel;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using IdBaseAuthService = IdentityBase.Services.AuthenticationService;

    public class ExternalController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly UserAccountService _userAccountService;
        private readonly IdBaseAuthService _authenticationService;
        private readonly IClientStore _clientStore;


        public ExternalController(
            IIdentityServerInteractionService interaction,
            IStringLocalizer localizer,
            ILogger<ExternalController> logger,
            IdentityBaseContext identityBaseContext,
            ApplicationOptions applicationOptions,
            UserAccountService userAccountService,
            IdBaseAuthService authenticationService,
             IClientStore clientStore)
        {
            // setting it this way since interaction service is null in the
            // base class oO
            this.InteractionService = interaction;
            this.Localizer = localizer;
            this.Logger = logger;
            this.IdentityBaseContext = identityBaseContext;
            this._applicationOptions = applicationOptions;
            this._userAccountService = userAccountService;
            this._authenticationService = authenticationService;
            this._clientStore = clientStore;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet(Name = "ExternalChallenge")]
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
        [HttpGet(Name = "ExternalCallback")]
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

            userAccount = await this._userAccountService
                .LoadByEmailAsync(email);

            if (userAccount == null)
            {
                // Create new user
                userAccount = await this._userAccountService
                    .CreateNewExternalUserAccountAsync(
                        provider,
                        subject,
                        email,
                        claims); 

                if (this._applicationOptions
                    .RequireExternalAccountVerification)
                {
                    // Send verification mail and display message
                    throw new NotImplementedException();
                }
                else
                {
                    // Update last authenticated flags
                    // Emit user authenticated event 

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
                    // Update last authenticated flags
                    // Emit user authenticated event 
                    // Sign in
                    // Check if additional information is required, if yes redirect to form 
                    // Redirect to return url
                    throw new NotImplementedException();
                }
                else
                {
                    // Ask user if he wants to merge accounts or use another account.
                    throw new NotImplementedException();
                }
            }
        }


        private async Task<IActionResult> SignInAsync(UserAccount userAccount, AuthenticateResult authResult, string provider)
        {
            // this allows us to collect any additonal claims or properties
            // for the specific prtotocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();

            this.ProcessLoginCallbackForOidc(
                authResult, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            //await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.SubjectId, user.Username));   */

            await HttpContext.SignInAsync(
                userAccount.Id.ToString(),
                userAccount.Email,
                provider,
                localSignInProps,
                additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(
                IdentityServer4.IdentityServerConstants
                    .ExternalCookieAuthenticationScheme);

            // retrieve return URL
            string returnUrl = authResult.Properties.Items["returnUrl"] ?? "~/";

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
            UserAccount userAccount = await _userAccountService
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

            // if the external provider issued an id_token, we'll keep it for signout
            string id_token = externalResult.Properties
                .GetTokenValue("id_token");

            if (id_token != null)
            {
                localSignInProps.StoreTokens(new[] {
                    new AuthenticationToken {
                        Name = "id_token", Value = id_token
                    }
                });
            }
        }




        /*
        [HttpGet("external/{provider}", Name = "External")]
        public IActionResult Index(string provider, string returnUrl)
        {
            return this.ChallengeExternalLogin(provider, returnUrl);
        }

        private async Task<IActionResult> IssueCookieAndRedirectAsync(
            UserAccount userAccount,
            string provider,
            string returnUrl,
            AuthenticateInfo info,
            List<Claim> claims)
        {
            var additionalClaims = new List<Claim>();

            // if the external system sent a session id claim, copy it over
            var sid = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                additionalClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            AuthenticationProperties props = null;
            var id_token = info.Properties.GetTokenValue("id_token");
            if (id_token != null)
            {
                props = new AuthenticationProperties();
                props.StoreTokens(new[] { new AuthenticationToken {
                    Name = "id_token", Value = id_token } });
            }

            // Issue authentication cookie for user
            await HttpContext.SignInAsync(
                userAccount.Id.ToString(), userAccount.Email, provider, props,
                additionalClaims.ToArray());

            // Delete temporary cookie used during external authentication
            await HttpContext.Authentication.SignOutAsync(
                IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // Validate return URL and redirect back to authorization endpoint
            if (_interaction.IsValidReturnUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("~/");
        }

        [HttpGet("external-callback")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var info = await HttpContext.GetAuthenticateInfoAsync(
                IdentityServerConstants.ExternalCookieAuthenticationScheme);
            var tempUser = info?.Principal;
            if (tempUser == null)
            {
                throw new Exception("External authentication error");
            }

            var claims = tempUser.Claims.ToList();

            var subjectClaim = claims
                .FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
            if (subjectClaim == null)
            {
                subjectClaim = claims
                    .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            }

            if (subjectClaim == null)
            {
                throw new Exception("Unknown user account ID");
            }

            claims.Remove(subjectClaim);

            var emailClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
            if (emailClaim == null)
            {
                throw new Exception("Unknown email");
            }

            var provider = subjectClaim.Issuer.ToLowerInvariant();
            var subject = subjectClaim.Value;
            var email = emailClaim.Value.ToLowerInvariant();

            var userAccount = await _userAccountService.LoadByExternalProviderAsync(
                provider, subject);
            if (userAccount != null)
            {
                await _userAccountService.UpdateLastUsedExternalAccountAsync(userAccount, provider, subject);
                return await IssueCookieAndRedirectAsync(userAccount, provider, returnUrl, info, claims);
            }
            else
            {
                userAccount = await _userAccountService.LoadByEmailWithExternalAsync(email);
                if (userAccount == null)
                {
                    userAccount = await _userAccountService.CreateNewExternalUserAccountAsync(
                        email, provider, subject, returnUrl);

                    if (_applicationOptions.RequireExternalAccountVerification)
                    {
                        // TODO: send confirmation mail and redirect to success page
                        throw new NotImplementedException();
                    }
                    else
                    {
                        await _userAccountService.UpdateLastUsedExternalAccountAsync(
                            userAccount, provider, subject);

                        return await IssueCookieAndRedirectAsync(userAccount,
                            provider, returnUrl, info, claims);
                    }
                }
                else
                {
                    if (_applicationOptions.AutomaticAccountMerge)
                    {
                        // Add external account to existing one
                        var externalAccount = _userAccountService.AddExternalAccountAsync(
                            userAccount.Id,
                            email,
                            provider,
                            subject
                        );

                        await _userAccountService.UpdateLastUsedExternalAccountAsync(
                            userAccount, provider, subject);

                        return await IssueCookieAndRedirectAsync(userAccount,
                            provider, returnUrl, info, claims);
                    }
                    else
                    {
                        // Ask user of he wants to join accounts or create new user
                        throw new NotImplementedException();
                    }
                }
            }
        }*/
    }
}