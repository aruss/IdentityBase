// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Web.ViewModels.Recover
{
    using System.Collections.Generic;
    using IdentityBase.Web.ViewModels.External;

    public class RecoverViewModel : IExternalLoginsViewModel
    {
        public string Email { get; set; }

        public bool EnableLocalLogin { get; set; }

        public bool EnableAccountRegistration { get; set; }

        public string LoginHint { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        public string ReturnUrl { get; set; }
    }
}