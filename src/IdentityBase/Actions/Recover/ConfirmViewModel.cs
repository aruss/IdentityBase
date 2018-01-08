// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Actions.Recover
{
    public class ConfirmViewModel : ConfirmInputModel
    {
        [DisplayName("Email")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}