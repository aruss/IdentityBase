// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    public class ChangeEmailConfirmViewModel
    {
        public string Email { get; set; }
        
        public string Key { get; set; }

        public string ReturnUrl { get; set; }
    }
}