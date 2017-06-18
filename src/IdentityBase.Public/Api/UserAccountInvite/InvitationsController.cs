using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Extensions;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceBase.Api;
using ServiceBase.Authorization;
using ServiceBase.Collections;
using ServiceBase.Notification.Email;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public.Api.UserAccountInvite
{
    [TypeFilter(typeof(ApiResultExceptionFilterAttribute))]
    [TypeFilter(typeof(ApiResultValidateModelAttribute))]
    public class InvitationsController : ApiController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClientStore _clientStore;

        public InvitationsController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IClientStore clientStore)
        {
            _userAccountService = userAccountService;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
            _clientStore = clientStore;
        }

        [HttpGet("invitations")]
        [ScopeAuthorize("useraccount.read")]
        public async Task<UserAccoutInviteListRespose> Get(UserAccoutInviteListRequest request)
        {
            var list = await _userAccountService.LoadInvitedUserAccountsAsync(request.Take, request.Skip);
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
        [ScopeAuthorize("useraccount.write")]
        public async Task<object> Put([FromBody]UserAccountInviteCreateRequest inputModel)
        {
            var client = await _clientStore.FindClientByIdAsync(inputModel.ClientId);

            if (client == null)
            {
                return BadRequest(new InvalidStateApiResult("The ClientId field is invalid.", ResponseMessageKind.Error, nameof(inputModel.ClientId)));
            }

            string returnUri;
            if (String.IsNullOrWhiteSpace(inputModel.ReturnUri) && client.RedirectUris.Count > 0)
            {
                returnUri = client.RedirectUris.First();
            }
            else if (client.RedirectUris.Contains(inputModel.ReturnUri))
            {
                returnUri = inputModel.ReturnUri;
            }
            else
            {
                return BadRequest(new InvalidStateApiResult("The ReturnUri field is invalid.", ResponseMessageKind.Error, nameof(inputModel.ReturnUri)));
            }

            if (inputModel.InvitedBy.HasValue)
            {
                if (await _userAccountService.LoadByEmailAsync(inputModel.Email) == null)
                {
                    return BadRequest(new InvalidStateApiResult("The InvitedBy field is invalid, UserAccount does not exists.", ResponseMessageKind.Error, nameof(inputModel.InvitedBy)));
                }
            }

            var userAccount = await _userAccountService.LoadByEmailAsync(inputModel.Email);
            if (userAccount != null)
            {
                return BadRequest(new InvalidStateApiResult("The Email field is invalid, UserAccount does not exists.", ResponseMessageKind.Error, nameof(inputModel.Email)));
            }

            userAccount = await _userAccountService.CreateNewLocalUserAccountAsync(inputModel.Email, inputModel.InvitedBy, returnUri);
            SendEmailAsync(userAccount);

            return new ApiResult
            {
                Success = true
            };
        }

        [HttpDelete("invitations/{UserAccountId}")]
        [ScopeAuthorize("useraccount.delete")]
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
            var baseUrl = ServiceBase.Extensions.StringExtensions.EnsureTrailingSlash(_httpContextAccessor.HttpContext.GetIdentityServerBaseUrl());
            await _emailService.SendEmailAsync(IdentityBaseConstants.EmailTemplates.UserAccountInvited, userAccount.Email, new
            {
                ConfirmUrl = $"{baseUrl}register/confirm/{userAccount.VerificationKey}",
                CancelUrl = $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
            }, true);
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
        /// <summary>
        /// Email address of a invited user
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// UserAccount id of the user who creates a invitation
        /// </summary>
        public Guid? InvitedBy { get; set; }

        /// <summary>
        /// Client id of the application where the user gets redirected
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        public string ReturnUri { get; set; }
    }

    public class UserAccountInviteCreateRespose
    {
    }
}