
namespace IdentityBase.WebApi.Actions.Foo
{
    using System.Threading.Tasks;
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;

    public class FooController : PublicApiController
    {
        [HttpGet("invitations")]
        [ScopeAuthorize("idbase", AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get()
        {
            return this.Ok(); 
        }
    }
}
