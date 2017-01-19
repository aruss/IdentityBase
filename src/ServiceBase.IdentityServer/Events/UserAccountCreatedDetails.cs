using System;

namespace ServiceBase.IdentityServer.Events
{
    /// <summary>
    /// Event data for <see cref="Models.UserAccount"/> creation
    /// </summary>
    public class UserAccountCreatedDetails
    {
        /// <summary>
        /// User accounts primary key
        /// </summary>
        public Guid UserAccountId { get; set; }

        /// <summary>
        /// Used Identity Provider
        /// </summary>
        public string Provider { get; set; }
    }

    /// <summary>
    /// Event data for <see cref="Models.UserAccount"/> update
    /// </summary>
    public class UserAccountUpdatedDetails
    {
        /// <summary>
        /// User accounts primary key
        /// </summary>
        public Guid UserAccountId { get; set; }
    }
}
