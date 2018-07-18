// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Logout
{
    using System.ComponentModel.DataAnnotations;

    public class LogoutInputModel
    {
        /// <summary>
        /// The logout identifier.
        /// </summary>
        [StringLength(50)]
        public string LogoutId { get; set; }
    }
}