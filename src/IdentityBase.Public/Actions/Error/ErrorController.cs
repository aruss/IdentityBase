namespace IdentityBase.Public.Actions.Error
{
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;

    public class ErrorController : Controller
    {
        private readonly IIdentityServerInteractionService interaction;

        public ErrorController(IIdentityServerInteractionService interaction)
        {
            this.interaction = interaction;
        }

        [Route("error", Name ="Error")]
        public async Task<IActionResult> Index(string errorId)
        {
            ErrorViewModel vm = new ErrorViewModel();

            if (errorId != null)
            {
                ErrorMessage message = await this.interaction
                    .GetErrorContextAsync(errorId);

                if (message != null)
                {
                    vm.Error = message;
                }
            }

            return this.View("Error", vm);
        }
    }
}
