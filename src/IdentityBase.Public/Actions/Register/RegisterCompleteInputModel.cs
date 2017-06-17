using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Register
{
    public class RegisterCompleteInputModel
    {
        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [StringLength(100)]
        public string PasswordConfirm { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}