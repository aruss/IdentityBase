// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.AspNetCore.Mvc;
    using ServiceBase.Mvc.ModelBinding;

    public class RecoverInputModel
    {
        [EmailAddress]
        [StringLength(254)]
        [Required(ErrorMessage = "The {0} field is required.")]
        [DataType(DataType.EmailAddress)]
        [DisplayName("Email")]
        [ModelBinder(BinderType = typeof(TrimStringModelBinder))]
        public string Email { get; set; }

        [Required]
        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}