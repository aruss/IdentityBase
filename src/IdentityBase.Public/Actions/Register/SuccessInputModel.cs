namespace IdentityBase.Public.Actions.Register
{
    using System.ComponentModel.DataAnnotations;

    public class SuccessInputModel
    {
        [StringLength(254)]
        public string Provider { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}