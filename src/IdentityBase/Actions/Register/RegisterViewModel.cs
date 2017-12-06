// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using IdentityBase.Models;
    using System.Collections.Generic;

    public class RegisterViewModel :
        RegisterInputModel,
        IExternalLoginsViewModel
    {
        public RegisterViewModel()
        {
        }

        public RegisterViewModel(RegisterInputModel inputModel)
        {
            this.Email = inputModel.Email;
            this.Password = inputModel.Password;
            this.PasswordConfirm = inputModel.PasswordConfirm;
            this.ReturnUrl = inputModel.ReturnUrl;
        }

        public bool EnableLocalLogin { get; set; }

        public bool EnableAccountRecover { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }       
    }
}