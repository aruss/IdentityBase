// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.Actions.Recover
{
    using System.ComponentModel.DataAnnotations;

    public class RecoverInputModel
    {
        [EmailAddress]
        [StringLength(254)]
        [Required]
        public string Email { get; set; }

        [Required]
        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}