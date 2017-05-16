using IdentityServer4.Events;
using System;

namespace IdentityBase.Events
{
    /// <summary>
    /// Event data for <see cref="Models.UserAccount"/> creation
    /// </summary>
    public class UserAccountCreatedEvent : Event 
    {
        public UserAccountCreatedEvent(string category, string name, EventTypes type, int id, string message = null)
            : base(category, name, type, id, message)
        {

        }

        /// <summary>
        /// User accounts primary key
        /// </summary>
        public Guid UserAccountId { get; set; }

        /// <summary>
        /// Used Identity Provider
        /// </summary>
        public string Provider { get; set; }
    }
}
