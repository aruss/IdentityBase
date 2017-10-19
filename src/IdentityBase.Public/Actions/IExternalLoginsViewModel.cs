namespace IdentityBase.Public.Actions
{
    using IdentityBase.Models;
    using System.Collections.Generic;
    
    public interface IExternalLoginsViewModel
    {
        string ReturnUrl { get; set; }
        IEnumerable<ExternalProvider> ExternalProviders { get; set; }
        IEnumerable<string> ExternalProviderHints { get; set; }
    }
}