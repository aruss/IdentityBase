using System;

namespace IdentityBase.EntityFramework.Entities
{
    public class UserAccountClaim
    {
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public string ValueType { get; set; }

        public UserAccount UserAccount { get; set; }

        public Guid UserAccountId { get; set; }

        public override bool Equals(object obj)
        {
            var item = obj as UserAccountClaim;

            if (item == null)
            {
                return false;
            }

            return this.Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}