using IdentityBase.Models;
using System.Collections.Generic;

namespace IdentityBase.Public.Actions.Recover
{
    public class RecoverViewModel : RecoverInputModel, IExternalLoginsViewModel
    {
        public bool EnableLocalLogin { get; set; }
        public bool EnableAccountRegistration { get; set; }

        public string LoginHint { get; set; }
        public IEnumerable<string> ExternalProviderHints { get; set; }
        public IEnumerable<ExternalProvider> ExternalProviders { get; set; }


        public RecoverViewModel()
        {
        }

        public RecoverViewModel(RecoverInputModel inputModel)
        {
            Email = inputModel.Email;
            ReturnUrl = inputModel.ReturnUrl;
        }  
    }
}