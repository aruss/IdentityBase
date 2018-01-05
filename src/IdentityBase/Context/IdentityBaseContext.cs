namespace IdentityBase
{
    using IdentityServer4.Models;
    using Microsoft.AspNetCore.Localization;

    public class IdentityBaseContext
    {
        public bool IsValid
        {
            get
            {
                return this.AuthorizationRequest != null;
            }
        }
        
        public AuthorizationRequest AuthorizationRequest { get; set; }

        public Client Client { get; set; }
    }
}