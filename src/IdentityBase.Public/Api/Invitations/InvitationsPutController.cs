namespace IdentityBase.Public.Api.Invitations
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using IdentityServer4.Extensions;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;
    using ServiceBase.Mvc;
    using ServiceBase.Notification.Email;

    [TypeFilter(typeof(ExceptionFilter))]
    [TypeFilter(typeof(ModelStateFilter))]
    [TypeFilter(typeof(BadRequestFilter))]
    public class InvitationsPutController : ApiController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IEmailService _emailService;
        private readonly IClientStore _clientStore;

        public InvitationsPutController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IClientStore clientStore)
        {
            this._userAccountService = userAccountService;
            this._emailService = emailService;
            this._clientStore = clientStore;
        }

        [HttpPut("invitations")]
        [ScopeAuthorize("idbase.invitations", AuthenticationSchemes =
             IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put(
            [FromBody]InvitationsPutInputModel inputModel)
        {
            Client client = await this._clientStore
                .FindClientByIdAsync(inputModel.ClientId);

            if (client == null)
            {
                return this.BadRequest(
                    nameof(inputModel.ClientId),
                    "The ClientId field is invalid."
                );
            }

            string returnUri;
            if (String.IsNullOrWhiteSpace(inputModel.ReturnUri) &&
                client.RedirectUris.Count > 0)
            {
                returnUri = client.RedirectUris.First();
            }
            else if (client.RedirectUris.Contains(inputModel.ReturnUri))
            {
                returnUri = inputModel.ReturnUri;
            }
            else
            {
                return this.BadRequest(
                    nameof(inputModel.ReturnUri),
                    "The ReturnUri field is invalid."
                );
            }

            if (inputModel.InvitedBy.HasValue)
            {
                if (await this._userAccountService
                    .LoadByEmailAsync(inputModel.Email) == null)
                {
                    return this.BadRequest(
                        nameof(inputModel.InvitedBy),
                        "The InvitedBy field is invalid, UserAccount does not exists."
                    );
                }
            }

            UserAccount userAccount = await _userAccountService
                .LoadByEmailAsync(inputModel.Email);

            if (userAccount != null)
            {
                return this.BadRequest(
                    nameof(inputModel.Email),
                    "The Email field is invalid, UserAccount already exists."
                );
            }

            userAccount = await this._userAccountService
                .CreateNewLocalUserAccountAsync(
                    inputModel.Email,
                    inputModel.InvitedBy,
                    returnUri
                );

            await this.SendEmailAsync(userAccount);

            this.Response.StatusCode = (int)HttpStatusCode.Created;
            return new ObjectResult(new InvitationsPutResultModel
            {
                Id = userAccount.Id,
                Email = userAccount.Email,
                CreatedAt = userAccount.CreatedAt,
                CreatedBy = userAccount.CreatedBy,
                VerificationKeySentAt = userAccount.VerificationKeySentAt
            });
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            string baseUrl = ServiceBase.Extensions.StringExtensions
                .EnsureTrailingSlash(this.HttpContext
                    .GetIdentityServerBaseUrl());

            await this._emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountInvited,
                userAccount.Email,
                new
                {
                    ConfirmUrl = $"{baseUrl}register/confirm/{userAccount.VerificationKey}",
                    CancelUrl = $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
                },
                true);
        }
    }
}