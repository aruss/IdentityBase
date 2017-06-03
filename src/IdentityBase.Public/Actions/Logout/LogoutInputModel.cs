using System.ComponentModel.DataAnnotations;

namespace IdentityBase.Public.Actions.Logout
{
    public class LogoutInputModel
    {
        [StringLength(50)]
        public string LogoutId { get; set; }
    }
}