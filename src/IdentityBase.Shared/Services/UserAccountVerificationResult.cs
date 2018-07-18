// Copyright (c) Russlan Akiev. All rights reserved. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for license information.

namespace IdentityBase.Services
{
    using IdentityBase.Models;

    public class UserAccountVerificationResult
    {
        /// <summary>
        /// User account that is trying to login.
        /// </summary>
        public UserAccount UserAccount { get; set; }

        /// <summary>
        /// Indicated if user has to change password, redirect the user to
        /// public password change page.
        /// </summary>
        public bool NeedChangePassword { get; set; }

        /// <summary>
        /// Indicated if user is allowed to login, possible cause is, user got banned. 
        /// </summary>
        public bool IsLoginAllowed { get; set; }

        /// <summary>
        /// Has user a local account, if false the user only has external accounts.
        /// </summary>
        public bool IsLocalAccount { get; set; }


        public bool IsPasswordValid { get; set; }
        public string[] Hints { get; internal set; }
    }
}