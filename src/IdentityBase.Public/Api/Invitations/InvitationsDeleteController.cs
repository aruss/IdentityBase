namespace IdentityBase.Public.Api.Invitations
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Api;
    using ServiceBase.Authorization;

    [TypeFilter(typeof(ApiResultExceptionFilterAttribute))]
    [TypeFilter(typeof(ApiResultValidateModelAttribute))]
    public class InvitationsDeleteController : ApiController
    {
        private readonly UserAccountService _userAccountService;

        public InvitationsDeleteController(
            UserAccountService userAccountService)
        {
            this._userAccountService = userAccountService;
        }
        
        [HttpDelete("invitations/{UserAccountId}")]
        [ScopeAuthorize("idbase.invitations", AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<object> Delete([FromRoute]Guid userAccountId)
        {
            UserAccount userAccount = await this._userAccountService
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

            await this._userAccountService.DeleteByIdAsync(userAccountId);

            return new ApiResult
            {
                Success = true
            };
        }
    }
}