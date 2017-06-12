using IdentityBase.Models;
using IdentityBase.Services;
using Microsoft.AspNetCore.Mvc;
using ServiceBase.Api;
using ServiceBase.Authorization;
using ServiceBase.Collections;
using ServiceBase.Notification.Email;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Extensions;
using ServiceBase.Extensions;
using System.Net.Http;

namespace IdentityBase.Public.Api.UserAccountInvite
{
    [TypeFilter(typeof(ApiResultExceptionFilterAttribute))]
    [TypeFilter(typeof(ApiResultValidateModelAttribute))]
    public class InvitationsController : ApiController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InvitationsController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userAccountService = userAccountService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("invitations")]
        //[ScopeAuthorize("invite.read")]
        public async Task<UserAccoutInviteListRespose> Get(UserAccoutInviteListRequest request)
        {
            var list = await _userAccountService.LoadInvitedUserAccounts(request.Take, request.Skip);
            var result = new UserAccoutInviteListRespose
            {
                Success = true,
                Result = new PagedList<object>
                {
                    Skip = list.Skip,
                    Take = list.Take,
                    Total = list.Total,
                    Sort = list.Sort,
                    Items = list.Items.Select(s => new
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

        [HttpPut("invitations")]
        //[ScopeAuthorize("invite.write")]
        public async Task<ApiResult> Put([FromBody]UserAccountInviteCreateRequest inputModel)
        {
            var inviterId = Guid.NewGuid();

            var userAccount = await _userAccountService.CreateNewLocalUserAccountAsync(inputModel.Email, inviterId);
            SendEmailAsync(userAccount);

            return new ApiResult
            {
                Success = true
            };
        }

        [HttpDelete("invitations/{UserAccountId}")]
        //[ScopeAuthorize("invite.delete")]
        public async Task<object> Delete([FromRoute]Guid userAccountId)
        {
            var userAccount = await _userAccountService.LoadByIdAsync(userAccountId);
            if (userAccount == null || userAccount.CreationKind != CreationKind.Invitation)
            {
                return this.NotFound();
            }

            if (userAccount.IsEmailVerified)
            {
                return this.BadRequest("Invitation is already confirmed and cannot be deleted"); 
            }
            
            await _userAccountService.DeleteByIdAsync(userAccountId);
            return new ApiResult
            {
                Success = true
            };
        }

        private async Task SendEmailAsync(UserAccount userAccount)
        {
            var baseUrl = _httpContextAccessor.HttpContext.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            await _emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountInvited, userAccount.Email, new
                {
                    ConfirmUrl = $"{baseUrl}invite/confirm/{userAccount.VerificationKey}",
                    CancelUrl = $"{baseUrl}invite/cancel/{userAccount.VerificationKey}"
                }
            );
        }
    }

    public class UserAccoutInviteListRequest : PagedListRequest
    {

    }

    public class UserAccoutInviteListRespose : ApiResult<PagedList<object>>
    {

    }

    public class UserAccountInviteCreateRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }
    }

    public class UserAccountInviteCreateRespose
    {

    }
}
