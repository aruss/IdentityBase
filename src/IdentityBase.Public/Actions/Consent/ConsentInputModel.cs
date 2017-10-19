namespace IdentityBase.Public.Actions.Consent
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ConsentInputModel
    {
        [StringLength(50)]
        public string Button { get; set; }
        public IEnumerable<string> ScopesConsented { get; set; }
        public bool RememberConsent { get; set; }
        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }
}
