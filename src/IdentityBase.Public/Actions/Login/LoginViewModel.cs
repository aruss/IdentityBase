using IdentityBase.Models;
using System.Collections.Generic;
using System.Linq;

namespace IdentityBase.Public.Actions.Login
{
    public class LoginViewModel : LoginInputModel, IExternalLoginsViewModel
    {
        public bool EnableLocalLogin { get; set; }
        public bool EnableRememberLogin { get; set; }
        public bool EnableAccountRegistration { get; set; }
        public bool EnableAccountRecover { get; set; }
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public string LoginHint { get; set; }
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        public IEnumerable<string> ExternalProviderHints { get; set; }

        public LoginViewModel()
        {
        }

        public LoginViewModel(LoginInputModel inputModel)
        {
            Email = inputModel.Email;
            Password = inputModel.Password;
            RememberLogin = inputModel.RememberLogin;
            ReturnUrl = inputModel.ReturnUrl;
        }
    }
}