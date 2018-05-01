namespace AspNetCoreWeb.Actions.Invite
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations;

    public class InviteInputModel
    {
        [Required]
        [EmailAddress]
        [Range(6, 254)]
        public string Email { get; set; }
    }
}
