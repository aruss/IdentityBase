// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.WebApi.Actions.Invitations
{
    using System;

    /// <summary>
    /// Represents output model for invitation creation
    /// </summary>
    public class InvitationsPutResultModel
    {
        /// <summary>
        /// ID of invited user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Email of invited user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User account ID of invitation host.
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Date time when user entity got created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date time when invitation email is sent to user.
        /// </summary>
        public DateTime? VerificationKeySentAt { get; set; }
    }
}