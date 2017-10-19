namespace IdentityBase.Public.Actions.Register
{
    using System.ComponentModel.DataAnnotations;

    public class RegisterInputModel
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage =
            "The password and confirmation password do not match.")]
        [StringLength(100)]
        public string PasswordConfirm { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}