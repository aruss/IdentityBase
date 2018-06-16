// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.UserAccounts
{
    using System.ComponentModel.DataAnnotations;

    public class ChangeEmailInputModel
    {
        /// <summary>
        /// Email address of a invited user
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// Client id of the application where the user gets redirected.
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// Return URI is used to redirect back to client, must be one of the
        /// clients white listed URIs.
        /// </summary>
        public string ReturnUri { get; set; }

        /// <summary>
        /// If true bypasses email verification and changes the email directly.
        /// </summary>
        public bool Force { get; set; }
    }
}