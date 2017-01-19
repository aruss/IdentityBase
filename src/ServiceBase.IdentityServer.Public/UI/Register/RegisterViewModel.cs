using System;
using System.Collections.Generic;
using ServiceBase.IdentityServer.Public.UI.Login;
using ServiceBase.IdentityServer.Models;

namespace ServiceBase.IdentityServer.Public.UI.Register
{
    public class RegisterViewModel : RegisterInputModel, IExternalLoginsViewModel
    {
        public string[] HintExternalAccounts { get; set; }
        public string ErrorMessage { get; set; }
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        public bool EnableLocalLogin { get; set; }

        public RegisterViewModel()
        {
        }

        public RegisterViewModel(RegisterInputModel other)
        {
            this.Email = other.Email;
            this.Password = other.Password;
            this.PasswordConfirm = other.PasswordConfirm;
        }
    }
}