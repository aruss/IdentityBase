// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class UserAccount
    {        
        public Guid Id { get; set; }

        // Maximum length of a valid email address is 254 characters.
        // See Dominic Sayers answer at SO: http://stackoverflow.com/a/574698/99240
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }

        /// <summary>
        /// Is user allowed to login with local account
        /// </summary>
        public bool IsLoginAllowed { get; set; }

        /// <summary>
        /// Last successfull login date
        /// </summary>
        public virtual DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Last failed login date
        /// </summary>
        public virtual DateTime? LastFailedLoginAt { get; set; }

        /// <summary>
        /// How many times user tried to login with invalid credentials
        /// </summary>
        public virtual int FailedLoginCount { get; set; }

        /// <summary>
        /// Hashed password
        /// </summary>
        [StringLength(200)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Last password change date
        /// </summary>
        public DateTime? PasswordChangedAt { get; set; }

        //public string Phone { get; set; }

        [StringLength(100)]
        public virtual string VerificationKey { get; set; }

        public virtual int? VerificationPurpose { get; set; }

        public virtual DateTime? VerificationKeySentAt { get; set; }

        [StringLength(2000)]
        public virtual string VerificationStorage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public IEnumerable<ExternalAccount> Accounts { get; set; }

        public IEnumerable<UserAccountClaim> Claims { get; set; }

        public Guid? CreatedBy { get; set; }
        public CreationKind CreationKind { get; set; }

    }
}
