using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Recover
{
    public class RecoverInputModel
    {
        [EmailAddress]
        [StringLength(254)]
        [Required]
        public string Email { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}