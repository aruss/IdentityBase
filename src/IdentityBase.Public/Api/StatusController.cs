using IdentityBase.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityBase.Public.Api
{
    public class StatusController : ApiController
    {
        private readonly ApplicationOptions _options; 

        public StatusController(ApplicationOptions options)
        {
            _options = options; 
        }

        /// <summary>
        /// Returns the configured state of the API 
        /// </summary>
        [HttpGet("status")]
        [AllowAnonymous]
        public async Task<Dictionary<string, string>> Get()
        {
            var status = new Dictionary<string, string>(); 

            if (_options.EnableUserInviteEndpoint)
            {
                status.Add("UserInviteEndpoint", $"{HttpContext.GetIdentityServerBaseUrl()}/api/users/_invite");
            }

            return await Task.FromResult(status); 
        }
    }
}
