using System;

namespace IdentityBase.EntityFramework.Entities
{
    public class ExternalAccount
    {
        public Guid UserAccountId { get; set; }

        public string Provider { get; set; }

        public string Subject { get; set; }

        public string Email { get; set; }

        public bool IsLoginAllowed { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UserAccount UserAccount { get; set; }
    }
}