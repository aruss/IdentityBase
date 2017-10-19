namespace IdentityBase.Public.Actions.Home
{
    using System.Threading.Tasks;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        private readonly IIdentityServerInteractionService interaction;

        public HomeController(IIdentityServerInteractionService interaction)
        {
            this.interaction = interaction;
        }

        /// <summary>
        /// Show landing page
        /// </summary>
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            // TODO: redirect to default RP since index page does not
            // provide any value
            return View();
        }
    }
}