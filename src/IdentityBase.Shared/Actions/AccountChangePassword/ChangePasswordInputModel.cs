// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.AccountChangePassword
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class ChangePasswordInputModel
    {
        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(100)]
        [DisplayName("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        [StringLength(100)]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "The {0} field is required.")]
        [StringLength(100)]
        public string PasswordCurrent { get; set; }
    }
}