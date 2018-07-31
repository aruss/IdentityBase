// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Models
{
    public class EmailProviderInfo
    {
        public string Provider { get; set; }
        public string WebMailBaseUrl { get; set; }
        public string[] Hosts { get; set; }
    }
}
