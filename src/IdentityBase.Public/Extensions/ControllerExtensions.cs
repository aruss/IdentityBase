using Microsoft.AspNetCore.Http.Authentication;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Mvc
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

    /// <summary>
    /// A base class for an api controller.
    /// </summary>
    public abstract class ApiController : ControllerBase
    {
    }    
}