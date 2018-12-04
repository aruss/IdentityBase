// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using IdentityBase.Actions.External;
    using IdentityBase.Forms;
    using IdentityBase.Models;

    public class LoginViewModel : IExternalProvidersViewModel
    {
        [EmailAddress]
        [StringLength(254)]
        [Required(ErrorMessage = "The {0} field is required.")]
        [DisplayName("Email")]
        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberLogin { get; set; }

        public bool EnableLocalLogin { get; set; }

        public bool EnableRememberMe { get; set; }

        public bool EnableAccountRegistration { get; set; }

        public bool EnableAccountRecover { get; set; }

        public bool IsExternalLoginOnly =>
            EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        public string LoginHint { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }

        public string ReturnUrl { get; set; }

        public CreateViewModelResult FormModel { get; set; }
    }
}