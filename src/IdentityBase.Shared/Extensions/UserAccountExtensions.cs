// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Extensions
{
    using IdentityBase.Models;
    using System;
    using System.Linq;

    public static partial class UserAccountExtensions
    {
        public static bool HasPassword(this UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentException(nameof(userAccount));
            }

            return !String.IsNullOrWhiteSpace(userAccount.PasswordHash);
        }

        public static bool HasExternalAccounts(this UserAccount userAccount)
        {
            return userAccount.Accounts != null &&
                userAccount.Accounts.Count() > 0;
        }

        public static bool IsNew(this UserAccount userAccount)
        {
            if (userAccount == null)
            {
                throw new ArgumentException(nameof(userAccount));
            }

            return !userAccount.LastLoginAt.HasValue;
        }

        // [Obsolete("Use new UserAccountService")]
        // public static void SetVerification(this UserAccount userAccount,
        //     string key,
        //     VerificationKeyPurpose purpose,
        //     string storage = null,
        //     DateTime? sentAt = null)
        // {
        //     if (userAccount == null)
        //     {
        //         throw new ArgumentException(nameof(userAccount));
        //     }
        // 
        //     if (key == null)
        //     {
        //         throw new ArgumentException(nameof(key));
        //     }
        // 
        //     userAccount.VerificationKey = key;
        //     userAccount.VerificationPurpose = (int)purpose;
        //     userAccount.VerificationKeySentAt = sentAt ?? DateTime.UtcNow;
        //     userAccount.VerificationStorage = storage;
        // }
        // 
        // [Obsolete("Use new UserAccountService")]
        // public static void ClearVerification(this UserAccount userAccount)
        // {
        //     userAccount.VerificationKey = null;
        //     userAccount.VerificationPurpose = null;
        //     userAccount.VerificationKeySentAt = null;
        //     userAccount.VerificationStorage = null;
        // }
    }
}
