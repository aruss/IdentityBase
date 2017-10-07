namespace IdentityBase.Public.Api.Invitations
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents input message for creating invitation
    /// </summary>
    public class InvitationsPutInputModel
    {
        /// <summary>
        /// Email address of a invited user
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// Client id of the application where the user gets redirected
        /// </summary>
        [Required]
        public string ClientId { get; set; }

        /// <summary>
        /// UserAccount id of the user who creates a invitation
        /// </summary>
        public Guid? InvitedBy { get; set; }
    
        /// <summary>
        /// Return URI is used to redirect back to client, must be one of the clients 
        /// white listed URIs
        /// </summary>
        public string ReturnUri { get; set; }
    }
}