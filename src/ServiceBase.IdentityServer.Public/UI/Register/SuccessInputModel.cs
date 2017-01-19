using System.ComponentModel.DataAnnotations;

namespace ServiceBase.IdentityServer.Public.UI.Register
{
    // TODO: together its longer than 2000 characters :/
    public class SuccessInputModel
    {
        [StringLength(254)]
        public string Provider { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}