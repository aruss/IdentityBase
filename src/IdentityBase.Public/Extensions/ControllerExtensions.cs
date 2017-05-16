using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;

namespace IdentityBase.Public.Extensions
{
    public static class ControllerExtensions
    {
        public static IActionResult ChallengeExternalLogin(this Controller controller, string provider, string returnUrl)
        {
            if (returnUrl != null)
            {
                returnUrl = UrlEncoder.Default.Encode(returnUrl);
            }

            returnUrl = "external-callback?returnUrl=" + returnUrl;

            // Start challenge and roundtrip the return URL
            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl,
                Items = { { "scheme", provider } }
            };

            return new ChallengeResult(provider, props);
        }
    }
}