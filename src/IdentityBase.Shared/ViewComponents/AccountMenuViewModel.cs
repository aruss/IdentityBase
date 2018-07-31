// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Mvc
{
    using System.Collections.Generic;

    public class AccountMenuViewModel
    {
        public string ReturnUrl { get; set; }
        public string ClientId { get; set; }
        public IEnumerable<AccountMenuItem> Items { get; set; }
    }
}
