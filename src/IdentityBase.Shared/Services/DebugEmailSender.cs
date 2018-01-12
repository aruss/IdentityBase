// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class DebugEmailSender : IEmailSender
    {
        private readonly ILogger<DebugEmailSender> _logger;

        public DebugEmailSender(ILogger<DebugEmailSender> logger)
        {
            this._logger = logger; 
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            this._logger.LogInformation(message); 
        }
    }
}