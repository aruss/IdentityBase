// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.UserAccounts
{
    using System;
    using System.Linq;
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

    public class ChangeEmailController : WebApiController
    {
        private readonly UserAccountService _userAccountService;
        private readonly IEmailService _emailService;
        private readonly IClientStore _clientStore;

        public ChangeEmailController(
            UserAccountService userAccountService,
            IEmailService emailService,
            IClientStore clientStore)
        {
            this._userAccountService = userAccountService;
            this._emailService = emailService;
            this._clientStore = clientStore;
        }

        [HttpPost("useraccounts/{UserAccountId}/change_email")]
        [ScopeAuthorize(WebApiConstants.ApiName, AuthenticationSchemes =
            IdentityServerAuthenticationDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post(
            [FromRoute]Guid userAccountId,
            [FromBody]ChangeEmailInputModel inputModel)
        {
            // Check if user account to change exists
            UserAccount userAccount = await this._userAccountService
                .LoadByIdAsync(userAccountId);

            if (userAccount == null)
            {
                return this.NotFound();
            }

            // Check if new email address is already taken
            if (await this._userAccountService
                .LoadByEmailAsync(inputModel.Email) != null)
            {
                return this.BadRequest(
                    nameof(inputModel.Email),
                    "The Email field is invalid, Email already taken."
                );
            }

            // Send confirmation mail to user 
            if (!inputModel.Force)
            {
                return await this.DoubleOptInAsync(userAccount, inputModel);
            }

            // Update user directly without sending confirmation email 
            await this._userAccountService.SetNewEmailAsync(
               userAccount,
               inputModel.Email.ToLower()
            );

            return this.Ok();
        }

        [NonAction]
        internal async Task<IActionResult> DoubleOptInAsync(
            UserAccount userAccount,
            ChangeEmailInputModel inputModel)
        {
            // Check if client exists
            Client client = await this._clientStore
               .FindClientByIdAsync(inputModel.ClientId);

            if (client == null)
            {
                return this.BadRequest(
                    nameof(inputModel.ClientId),
                    "The ClientId field is invalid."
                );
            }

            // Check if returnUrl is valid 
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

            // Alrighty then, create a confirmation token.
            await this._userAccountService
                .SetEmailChangeVirificationKeyAsync(
                    userAccount,
                    inputModel.Email,
                    returnUri
                );

            await this.SendEmailAsync(
                inputModel.Email,
                userAccount.VerificationKey
            );

            return this.Ok();
        }

        [NonAction]
        internal async Task SendEmailAsync(
            string newEmail,
            string verificationKey)
        {
            string baseUrl = ServiceBase.Extensions.StringExtensions
                .RemoveTrailingSlash(this.HttpContext
                    .GetIdentityServerBaseUrl());

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountEmailChanged,
                newEmail,
                new
                {
                    ConfirmUrl =
                        $"{baseUrl}/email/confirm/{verificationKey}",

                    CancelUrl =
                        $"{baseUrl}/email/cancel/{verificationKey}"
                },
                true
            );
        }
    }
}