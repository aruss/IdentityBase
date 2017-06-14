using IdentityServer4.Events;
using System;

namespace IdentityBase.Events
{
    /// <summary>
    /// Event data for <see cref="Models.UserAccount"/> creation
    /// </summary>
    public class UserAccountInvitedEvent : Event 
    {
        public UserAccountInvitedEvent(string category, string name, EventTypes type, int id, string message = null)
            : base(category, name, type, id, message)
        {

        }

        /// <summary>
        /// User accounts primary key
        /// </summary>
        public Guid UserAccountId { get; set; }

        /// <summary>
        /// User acccounts who invited primary key
        /// </summary>
        public Guid? InvitedByUserAccountId { get; set; }
    }
}
