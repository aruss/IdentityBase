// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using System.Globalization;
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using ServiceBase.Extensions;
    using ServiceBase.Notification.Email;

    /// <summary>
    /// Sends notifications to user in form of push/email/sms
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationOptions _options;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActionContextAccessor _actionContextAccessor;

        public NotificationService(
            ApplicationOptions options,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IActionContextAccessor actionContextAccessor)
        {
            this._options = options;
            this._emailService = emailService;
            this._httpContextAccessor = httpContextAccessor;
            this._actionContextAccessor = actionContextAccessor;
        }

        /// <summary>
        /// Send an email with confirmation and cancelation links in
        /// case of account creation.
        /// </summary>
        /// <param name="userAccount">The instance of created
        /// <see cref="UserAccount"/>.</param>
        public async Task SendUserAccountCreatedEmailAsync(
           UserAccount userAccount)
        {
            string baseUrl = this._httpContextAccessor
                 .HttpContext
                 .GetBaseUrl()
                 .RemoveTrailingSlash();

            IdentityBaseContext idbContext = this._httpContextAccessor
                 .HttpContext.GetIdentityBaseContext();

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountCreated,
                userAccount.Email,
                new
                {
                    ConfirmUrl = this.GetUrl(
                        baseUrl,
                        "/register/confirm",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),

                    CancelUrl = this.GetUrl(
                        baseUrl,
                        "/register/cancel",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),
                },
                true
            );
        }

        public async Task SendUserAccountRecoverEmailAsync(
            UserAccount userAccount)
        {
            string baseUrl = this._httpContextAccessor
                 .HttpContext
                 .GetBaseUrl()
                 .RemoveTrailingSlash();

            IdentityBaseContext idbContext = this._httpContextAccessor
                 .HttpContext.GetIdentityBaseContext();

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountRecover,
                userAccount.Email,
                new
                {
                    ConfirmUrl = this.GetUrl(
                        baseUrl,
                        "/recover/confirm",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),

                    CancelUrl = this.GetUrl(
                        baseUrl,
                        "/recover/cancel",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),
                },
                true
            );
        }

        public async Task SendUserAccountInvitationEmailAsync(
            UserAccount userAccount)
        {
            string baseUrl = this._httpContextAccessor
                 .HttpContext
                 .GetBaseUrl()
                 .RemoveTrailingSlash();

            IdentityBaseContext idbContext = this._httpContextAccessor
                 .HttpContext.GetIdentityBaseContext();

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountInvited,
                userAccount.Email,
                new
                {
                    ConfirmUrl = this.GetUrl(
                        baseUrl,
                        "/register/confirm",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),

                    CancelUrl = this.GetUrl(
                        baseUrl,
                        "/register/cancel",
                        userAccount.VerificationKey,
                        idbContext.Client.ClientId),
                },
                true
            );
        }

        private string GetUrl(
             string baseUrl,
             string path,
             string verificationKey,
             string clientId)
        {
            return $"{baseUrl}{path}?key={verificationKey}&clientId={clientId}&culture={CultureInfo.CurrentUICulture.Name}";
        }
    }
}
