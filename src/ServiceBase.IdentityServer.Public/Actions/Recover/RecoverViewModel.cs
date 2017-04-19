using ServiceBase.IdentityServer.Public.UI.Login;
using System.Collections.Generic;
using System;
using ServiceBase.IdentityServer.Models;

namespace ServiceBase.IdentityServer.Public.UI.Recover
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