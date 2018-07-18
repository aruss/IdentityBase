// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Consent
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ConsentInputModel
    {
        [StringLength(50)]
        public string Button { get; set; }

        public IEnumerable<string> ScopesConsented { get; set; }

        public bool RememberConsent { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}
