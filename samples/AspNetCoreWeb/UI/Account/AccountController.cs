using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AspNetCoreWeb.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Logout()
        {
            //await HttpContext.Authentication.SignOutAsync("Cookies");
            //await HttpContext.Authentication.SignOutAsync("oidc");

            return new SignOutResult(new[] { "Cookies", "oidc" });
        }
    }
}