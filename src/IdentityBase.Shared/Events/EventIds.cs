// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Events
{
    /// <summary>
    /// IDs for messenging events 
    /// </summary>
    public static class EventIds
    {
        /// <summary>
        /// If user signs up
        /// </summary>
        public const int UserAccountCreated = 4000;
        public const int UserAccountLogin = 4100;
        public const int UserAccountInvited = 4200;

        public const int UserAccountDeleted = 4200;
    }
}

