using System;
using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Register
{
    public class RegisterCompleteViewModel : RegisterCompleteInputModel
    {
        public Guid UserAccountId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        public RegisterCompleteViewModel()
        {

        }

        public RegisterCompleteViewModel(RegisterCompleteInputModel inputModel)
        {
            Password = inputModel.Password;
            PasswordConfirm = inputModel.PasswordConfirm;
            ReturnUrl = inputModel.ReturnUrl; 
        }
    }
}