using IdentityBase.Models;
using System.Collections.Generic;

namespace IdentityBase.Public.Actions.Register
{
    public class RegisterViewModel : RegisterInputModel, IExternalLoginsViewModel
    {
        public bool EnableLocalLogin { get; set; }
        public bool EnableAccountRecover { get; set; }
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        public IEnumerable<string> ExternalProviderHints { get; set; }

        public RegisterViewModel()
        {
        }

        public RegisterViewModel(RegisterInputModel inputModel)
        {
            Email = inputModel.Email;
            Password = inputModel.Password;
            PasswordConfirm = inputModel.PasswordConfirm;
            ReturnUrl = inputModel.ReturnUrl;
        }
    }
}