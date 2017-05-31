using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityBase.Public.Api
{
    public class StatusController : PublicApiController
    {
        public StatusController()
        {

        }

        [Route("status")]
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
