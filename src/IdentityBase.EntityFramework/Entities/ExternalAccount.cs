using System;

namespace IdentityBase.EntityFramework.Entities
{
    public class ExternalAccount
    {
        public Guid UserAccountId { get; set; }

        public string Provider { get; set; }

        public string Subject { get; set; }

        public string Email { get; set; }
        
        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public UserAccount UserAccount { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as ExternalAccount;

            if (item == null)
            {
                return false;
            }

            return this.Provider.Equals(item.Provider) &&
               this.Subject.Equals(item.Subject);
        }

        public override int GetHashCode()
        {
            return this.Provider.GetHashCode() ^
                this.Subject.GetHashCode(); 
        }
    }
}