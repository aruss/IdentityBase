// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using System.ComponentModel.DataAnnotations;

    public class SuccessInputModel
    {
        [StringLength(254)]
        public string Provider { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}