namespace IdentityBase.Public.Api.Invitations
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Api;
    using ServiceBase.Authorization;
    using ServiceBase.Collections;
    using ServiceBase.Notification.Email;

    [TypeFilter(typeof(ApiResultExceptionFilterAttribute))]
    [TypeFilter(typeof(ApiResultValidateModelAttribute))]
    public class InvitationsGetController : ApiController
    {
        private readonly UserAccountService _userAccountService;

        public InvitationsGetController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IClientStore clientStore)
        {
            this._userAccountService = userAccountService;
        }

        [HttpGet("invitations")]
        [ScopeAuthorize("idbase.invitations", AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ApiResult<PagedList<InvitationsPutResultModel>>> Get(
            PagedListRequest request)
        {
            PagedList<UserAccount> list = await this._userAccountService
                .LoadInvitedUserAccountsAsync(request.Take, request.Skip);

            var result = new ApiResult<PagedList<InvitationsPutResultModel>>
            {
                Success = true,
                Result = new PagedList<InvitationsPutResultModel>
                {
                    Skip = list.Skip,
                    Take = list.Take,
                    Total = list.Total,
                    Sort = list.Sort,
                    Items = list.Items.Select(s => new InvitationsPutResultModel
                    {
                        Id = s.Id,
                        Email = s.Email,
                        CreatedAt = s.CreatedAt,
                        CreatedBy = s.CreatedBy,
                        VerificationKeySentAt = s.VerificationKeySentAt
                    }).ToArray()
                }
            };

            return result;
        }
    }
}