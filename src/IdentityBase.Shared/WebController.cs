namespace IdentityBase.Web
{
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public abstract class WebController : Controller
    {
        public IStringLocalizer Localizer { get; set; }
        public ILogger Logger { get; set; }
        public IIdentityServerInteractionService InteractionService { get; set; }
        
        public void AddModelError(string message)
        {
            this.ModelState.AddModelError(this.Localizer[message]);
        }
    }
}
