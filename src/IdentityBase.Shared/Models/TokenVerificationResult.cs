// Copyright (c) Russlan Akiev. All rights reserved. Licensed under the Apache License, Version 2.0.
// See LICENSE in the project root for license information.

namespace IdentityBase.Models
{
    public class TokenVerificationResult
    {
        public UserAccount UserAccount { get; set; }
        public bool TokenExpired { get; set; }
        public bool PurposeValid { get; set; }
    }
}