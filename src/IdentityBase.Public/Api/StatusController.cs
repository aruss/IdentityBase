using Microsoft.AspNetCore.Mvc;
using ServiceBase.Authorization;

namespace IdentityBase.Public.Api
{
    public class StatusController : PublicApiController
    {
        public StatusController()
        {

        }

        [Route("status")]
        [HttpGet]
        [ScopeAuthorize("api1")]
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
