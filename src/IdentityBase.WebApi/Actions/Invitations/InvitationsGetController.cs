// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.Invitations
{
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using IdentityBase.Services;
    using IdentityServer4.AccessTokenValidation;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Authorization;
    using ServiceBase.Collections;
    using ServiceBase.Notification.Email;

    public class InvitationsGetController : WebApiController
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
    }
}