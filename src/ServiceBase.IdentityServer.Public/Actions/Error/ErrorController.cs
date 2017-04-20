using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.Actions.Error
{
    public class ErrorController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;

        public ErrorController(IIdentityServerInteractionService interaction)
        {
            _interaction = interaction;
        }

        [Route("error", Name ="Error")]
        public async Task<IActionResult> Index(string errorId)
        {
            var vm = new ErrorViewModel();

            if (errorId != null)
            {
                var message = await _interaction.GetErrorContextAsync(errorId);
                if (message != null)
                {
                    vm.Error = message;
                }
            }

            return View("Error", vm);
        }
    }
}
