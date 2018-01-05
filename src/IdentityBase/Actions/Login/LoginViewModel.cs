// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Login
{
    using System.Collections.Generic;
    using System.Linq;
    using IdentityBase.Models;

    public class LoginViewModel : LoginInputModel, IExternalLoginsViewModel
    {
        public LoginViewModel()
        {
        }

        public LoginViewModel(LoginInputModel inputModel)
        {
            this.Email = inputModel.Email;
            this.Password = inputModel.Password;
            this.RememberLogin = inputModel.RememberLogin;
            this.ReturnUrl = inputModel.ReturnUrl;
        }

            
        public bool EnableLocalLogin { get; set; }

        public bool EnableRememberLogin { get; set; }

        public bool EnableAccountRegistration { get; set; }

        public bool EnableAccountRecover { get; set; }

        public bool IsExternalLoginOnly =>
            EnableLocalLogin == false && ExternalProviders?.Count() == 1;

        public string LoginHint { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }    
    }
}