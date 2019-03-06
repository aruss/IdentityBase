namespace AspNetCoreApi.Controllers
{
    using System.Linq;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("/")]
    public class IndexController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new JsonResult(new
            {
                name = "AspNetCoreApi"
            });
        }
    }
}