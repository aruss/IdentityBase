using System;

namespace IdentityBase.Public.Api.UserAccountInvite
{
    public class UserAccountDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? VerificationKeySentAt { get; set; }
    }
}