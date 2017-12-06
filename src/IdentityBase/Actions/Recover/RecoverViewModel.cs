// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Recover
{
    using IdentityBase.Models;
    using System.Collections.Generic;

    public class RecoverViewModel : RecoverInputModel, IExternalLoginsViewModel
    {
        public bool EnableLocalLogin { get; set; }

        public bool EnableAccountRegistration { get; set; }

        public string LoginHint { get; set; }

        public IEnumerable<string> ExternalProviderHints { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        
        public RecoverViewModel(RecoverInputModel inputModel)
        {
            this.Email = inputModel.Email;
            this.ReturnUrl = inputModel.ReturnUrl;
        }  
    }
}