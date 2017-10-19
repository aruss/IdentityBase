namespace IdentityBase.Public.Actions.Logout
{
    using System.ComponentModel.DataAnnotations;

    public class LogoutInputModel
    {
        [StringLength(50)]
        public string LogoutId { get; set; }
    }
}