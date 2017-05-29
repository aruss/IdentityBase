using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityBase.Public.Api
{
    //[Area("PublicApi")]
    public class StatusController : Controller
    {
        public StatusController()
        {

        }
        
        [Route("api/status")]
        [HttpGet]
        [Authorize("api1")]
        public object Get()
        {
            return new
            {
                Id = "foo",
                Name = "Bar"
            }; 
        }
    }
}
