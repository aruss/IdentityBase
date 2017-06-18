using IdentityBase.Models;
using System.Collections.Generic;

namespace IdentityBase.Public.Actions
{
    public interface IExternalLoginsViewModel
    {
        string ReturnUrl { get; set; }
        IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        IEnumerable<string> ExternalProviderHints { get; set; }
    }
}