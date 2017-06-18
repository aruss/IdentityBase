using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Recover
{
    public class SuccessInputModel
    {
        [StringLength(254)]
        public string Provider { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}