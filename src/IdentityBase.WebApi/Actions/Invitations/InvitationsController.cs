// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.Invitations
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using IdentityBase.Extensions;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using IdentityServer4.Models;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;
    using ServiceBase.Collections;
    using ServiceBase.Mvc;
    using ServiceBase.Notification.Email;

    public class InvitationsController : WebApiController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IEmailService _emailService;
        private readonly IClientStore _clientStore;
        private readonly NotificationService _notificationService;

        public InvitationsController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IClientStore clientStore,
            NotificationService notificationService)
        {
            this._userAccountService = userAccountService;
            this._emailService = emailService;
            this._clientStore = clientStore;
            this._notificationService = notificationService;
        }

        [HttpGet("invitations")]
        [ScopeAuthorize(WebApiConstants.ApiName, AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get(PagedListInputModel request)
        {
            PagedList<UserAccount> list = await this._userAccountService
                .LoadInvitedUserAccountsAsync(request.Take, request.Skip);

            var result = new PagedList<InvitationsPutResultModel>
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
            };

            return new ObjectResult(result);
        }

        [HttpPut("invitations")]
        [ScopeAuthorize(WebApiConstants.ApiName, AuthenticationSchemes =
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

            string returnUri = client.TryGetReturnUri(inputModel.ReturnUri);

            if (String.IsNullOrWhiteSpace(returnUri))
            {
                return this.BadRequest(
                    nameof(inputModel.ReturnUri),
                    "The ReturnUri field is invalid."
                );
            }

            UserAccount invitedByUserAccount = null;
            if (inputModel.InvitedBy.HasValue)
            {
                invitedByUserAccount = await this._userAccountService
                    .LoadByIdAsync(inputModel.InvitedBy.Value);

                if (invitedByUserAccount == null)
                {
                    return this.BadRequest(
                        nameof(inputModel.InvitedBy),
                        "The InvitedBy field is invalid, UserAccount does not exists."
                    );
                }
            }

            UserAccount userAccount = await this._userAccountService
                .LoadByEmailAsync(inputModel.Email);

            if (userAccount != null)
            {
                return this.BadRequest(
                    nameof(inputModel.Email),
                    "The Email field is invalid, Email already taken."
                );
            }

            userAccount = await this._userAccountService
                .CreateNewLocalUserAccountAsync(
                    inputModel.Email,
                    returnUri,
                    invitedByUserAccount
                );

            await this._notificationService
                .SendUserAccountInvitationEmailAsync(userAccount);

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

        [HttpDelete("invitations/{UserAccountId}")]
        [ScopeAuthorize(WebApiConstants.ApiName, AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Delete([FromRoute]Guid userAccountId)
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

            return this.Ok();
        }
    }
}