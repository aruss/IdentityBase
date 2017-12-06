namespace IdentityBase.EntityFramework.Entities
{
    using System;
    using System.Collections.Generic;

    public class UserAccount
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public bool IsEmailVerified { get; set; }

        public DateTime? EmailVerifiedAt { get; set; }

        public bool IsLoginAllowed { get; set; }

        public virtual DateTime? LastLoginAt { get; set; }

        public virtual DateTime? LastFailedLoginAt { get; set; }

        public virtual int FailedLoginCount { get; set; }

        public string PasswordHash { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        public virtual string VerificationKey { get; set; }

        public virtual int? VerificationPurpose { get; set; }

        public virtual DateTime? VerificationKeySentAt { get; set; }

        public virtual string VerificationStorage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<ExternalAccount> Accounts { get; set; }

        public List<UserAccountClaim> Claims { get; set; }

        public Guid? CreatedBy { get; set; }

        public int CreationKind { get; set; }
    }
}