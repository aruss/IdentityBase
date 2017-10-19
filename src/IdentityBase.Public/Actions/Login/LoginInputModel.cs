namespace IdentityBase.Public.Actions.Login
{
    using System.ComponentModel.DataAnnotations;

    public class LoginInputModel
    {
        [EmailAddress]
        [StringLength(254)]
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}