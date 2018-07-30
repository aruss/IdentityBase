// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    public class ProfileViewModel
    {       
        public string ClientId { get; set; }

        public string Email { get; set; }
        public bool IsEmailVerified { get; set; }

        // public string Phone { get; set; }
    }
}