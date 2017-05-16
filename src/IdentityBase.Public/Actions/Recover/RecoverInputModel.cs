using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Recover
{
    public class RecoverInputModel
    {
        [Required]
        public string Email { get; set; }

        public string ReturnUrl { get; set; }
    }
}