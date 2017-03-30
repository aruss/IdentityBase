using IdentityServer4.Events;
using System;

namespace ServiceBase.IdentityServer.Events
{
    /// <summary>
    /// Event data for <see cref="Models.UserAccount"/> update
    /// </summary>
    public class UserAccountUpdatedEvent : Event
    {
        public UserAccountUpdatedEvent(string category, string name, EventTypes type, int id, string message = null) 
            : base(category, name, type, id, message)
        {
        }

        /// <summary>
        /// User accounts primary key
        /// </summary>
        public Guid UserAccountId { get; set; }
    }
}
