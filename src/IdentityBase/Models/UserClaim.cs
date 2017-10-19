namespace IdentityBase.Models
{
    using System;

    public class UserAccountClaim
    {
        public UserAccountClaim()
        {

        }

        public UserAccountClaim(
            string type,
            string value,
            string valueType = null)
        {
            this.Type = type;
            this.Value = value;
            this.ValueType = valueType;
        }

        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }

        public UserAccount UserAccount { get; set; }
        public Guid UserAccountId { get; set; }
    }
}
