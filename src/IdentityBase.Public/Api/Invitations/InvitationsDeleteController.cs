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
    public class InvitationsDeleteController : ApiController
    {
        private readonly UserAccountService userAccountService;

        public InvitationsDeleteController(
            UserAccountService userAccountService)
        {
            this.userAccountService = userAccountService;
        }
        
        [HttpDelete("invitations/{UserAccountId}")]
        [ScopeAuthorize("idbase", AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Delete([FromRoute]Guid userAccountId)
        {
            UserAccount userAccount = await this.userAccountService
                .LoadByIdAsync(userAccountId);

            if (userAccount == null ||
                userAccount.CreationKind != CreationKind.Invitation)
            {
                return this.NotFound();
            }

            if (userAccount.IsEmailVerified)
            {
                return this.BadRequest(
                    "Invitation is already confirmed and cannot be deleted");
            }

            await this.userAccountService.DeleteByIdAsync(userAccountId);

            return this.Ok(); 
        }
    }
}