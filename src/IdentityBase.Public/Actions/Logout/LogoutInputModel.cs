// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Public.Actions.Logout
{
    using System.ComponentModel.DataAnnotations;

    public class LogoutInputModel
    {
        [StringLength(50)]
        public string LogoutId { get; set; }
    }
}