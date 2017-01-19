using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.UI.Home
{
    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        /// <summary>
        /// Show landing page
        /// </summary>
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            // TODO: redirect to default RP since index page does not provide any value

            return View();
        }
    }
}