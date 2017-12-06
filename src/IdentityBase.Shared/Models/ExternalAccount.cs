// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class ExternalAccount
    {
        public Guid UserAccountId { get; set; }

        public string Provider { get; set; }

        public string Subject { get; set; }

        // Maximum length of a valid email address is 254 characters.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// Is user allowed to login with this external account
        /// </summary>
        public bool IsLoginAllowed { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UserAccount UserAccount { get; set; }
    }
}
