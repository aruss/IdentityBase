using IdentityBase.Public.Actions.Login;
using System.Collections.Generic;
using System;
using IdentityBase.Models;

namespace IdentityBase.Public.Actions.Recover
{
    public class RecoverViewModel : RecoverInputModel, IExternalLoginsViewModel
    {
        public RecoverViewModel()
        {
        }

        public RecoverViewModel(RecoverInputModel other)
        {
            this.Email = other.Email;
        }

        public bool EnableLocalLogin { get; set; }

        public string ErrorMessage { get; set; }

        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
    }
}