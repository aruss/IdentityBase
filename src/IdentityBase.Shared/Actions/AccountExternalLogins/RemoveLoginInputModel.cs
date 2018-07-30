// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System.ComponentModel.DataAnnotations;

    public class RemoveLoginInputModel
    {
        [StringLength(254)]
        [Required]
        public string Provider { get; set; }       
    }
}
