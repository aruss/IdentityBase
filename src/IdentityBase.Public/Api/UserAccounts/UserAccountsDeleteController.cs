namespace IdentityBase.Public.Api.Invitations
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;
    using ServiceBase.Mvc;

    [TypeFilter(typeof(ExceptionFilter))]
    [TypeFilter(typeof(ModelStateFilter))]
    [TypeFilter(typeof(BadRequestFilter))]
    public class UserAccountDeleteController : ApiController
    {
        private readonly UserAccountService userAccountService;

        public UserAccountDeleteController(
            UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        [HttpDelete("useraccounts/{UserAccountId}")]
        [ScopeAuthorize("idbase.useraccounts", AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Delete([FromRoute]Guid userAccountId)
        {
            UserAccount userAccount = await this.userAccountService
                .LoadByIdAsync(userAccountId);

            if (userAccount == null)
            {
                return this.NotFound();
            }
            
            await this.userAccountService.DeleteByIdAsync(userAccountId);

            return this.Ok(); 
        }
    }
}