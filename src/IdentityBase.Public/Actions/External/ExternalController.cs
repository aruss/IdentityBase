
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityBase.Configuration;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityModel;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using IdentityServer4;

namespace IdentityBase.Public.Actions.Login
{
    public class ExternalController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<ExternalController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;

        public ExternalController(
            ApplicationOptions applicationOptions,
            ILogger<ExternalController> logger,
            IIdentityServerInteractionService interaction,
            UserAccountService userAccountService)
        {
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _userAccountService = userAccountService;
        }

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
        }
    }
}