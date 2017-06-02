using IdentityBase.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Mvc;
using ServiceBase.Authorization;
using System.Collections.Generic;

namespace IdentityBase.Public.Api
{
    public class StatusController : ApiController
    {
        public StatusController(ApplicationOptions options)
        {

        }

        [Route("status")]
        [HttpGet]
        [ScopeAuthorize("api1")]
        public object Get()
        {
            var status = new Dictionary<string, string>(); 

            return new
            {
                UserInviteEndpoint = HttpContext.GetIdentityServerBaseUrl(),
                Name = "Bar"
            };
        }
    }
}
