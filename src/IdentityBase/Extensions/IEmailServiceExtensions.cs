// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Extensions
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using ServiceBase.Notification.Email;

    public static class IEmailServiceExtensions
    {
        public async static Task SendAccountCreatedEmailAsync(
            this IEmailService emailService,
            UserAccount userAccount)
        {
            await emailService.SendEmailAsync(
                "AccountCreated",
                userAccount.Email,
                new
                {
                    Token = userAccount.VerificationKey
                },
                true);
        }
    }
}
