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

        public bool IsActive { get; set; }

        /*public bool IsPhoneVerified { get; set; }

        public DateTime? PhoneVerifiedAt { get; set; }

        public bool IsPhoneAllowed { get; set; }*/

        public DateTime? LastLoginAt { get; set; }

        public DateTime? LastFailedLoginAt { get; set; }

        public int FailedLoginCount { get; set; }

        public string PasswordHash { get; set; }

        public DateTime? PasswordChangedAt { get; set; }

        public string VerificationKey { get; set; }

        public int? VerificationPurpose { get; set; }

        public DateTime? VerificationKeySentAt { get; set; }

        public string VerificationStorage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public List<ExternalAccount> Accounts { get; set; }

        public List<UserAccountClaim> Claims { get; set; }

        public Guid? CreatedBy { get; set; }

        public int CreationKind { get; set; }
    }
}