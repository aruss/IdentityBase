// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Models
{
    public enum VerificationKeyPurpose
    {
        ResetPassword,
        ChangeEmail,
        ChangeMobile,
        ConfirmAccount,
        MergeAccount
    }

    public enum CreationKind
    {
        /// <summary>
        /// Created via frontend by user it self.
        /// </summary>
        SelfService,

        /// <summary>
        /// User was invited via another user. 
        /// </summary>
        Invitation
    }
}
