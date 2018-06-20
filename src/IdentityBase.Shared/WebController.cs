namespace IdentityBase.Web
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public abstract class WebController : Controller
    {
        public IStringLocalizer Localizer { get; private set; }
        public ILogger Logger { get; private set; } 

        public WebController(IStringLocalizer localizer, ILogger logger)
        {
            this.Localizer = localizer;
            this.Logger = logger; 
        }

        public WebController()
        {

        }
    }
}
