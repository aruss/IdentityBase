using ServiceBase.IdentityServer.Models;
using System.Collections.Generic;
using System.Linq;

namespace ServiceBase.IdentityServer.Public.Actions.Login
{
    public class LoginViewModel : LoginInputModel, IExternalLoginsViewModel
    {
        public LoginViewModel()
        {
        }

        public LoginViewModel(LoginInputModel other)
        {
            Email = other.Email;
            Password = other.Password;
            RememberLogin = other.RememberLogin;
            ReturnUrl = other.ReturnUrl;
        }

        public string ErrorMessage { get; set; }
        public string InfoMessage { get; set; }
        public bool EnableLocalLogin { get; set; }
        public bool EnableRememberLogin { get; set; }
        public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        public IEnumerable<string> LoginHints { get; set; }
    }

    public interface IExternalLoginsViewModel
    {
        IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        bool EnableLocalLogin { get; set; }
        string ReturnUrl { get; set; }
    }
}