// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using System.Collections.Generic;
    using IdentityBase.Actions.External;
    using IdentityBase.Forms;
    using IdentityBase.Models;

    public class RecoverViewModel : IExternalProvidersViewModel
    {
        public string Email { get; set; }

        public bool EnableLocalLogin { get; set; }

        public bool EnableAccountRegistration { get; set; }

        public string LoginHint { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        public string ReturnUrl { get; set; }

        public CreateViewModelResult FormModel { get; set; }
    }
}