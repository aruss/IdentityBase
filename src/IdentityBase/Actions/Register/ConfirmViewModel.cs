// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    public class ConfirmViewModel : ConfirmInputModel
    {
        public bool RequiresPassword { get; set; }

        public string Email { get; set; }
    }
}