// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Events
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using ServiceBase.Events;

    /// <summary>
    /// Event for successful <see cref="UserAccount"/> invitation.
    /// </summary>
    public class UserAccountInvitedSuccessEvent : Event 
    {
        /// <summary>
        /// Created a instace of <see cref="UserAccountInvitedSuccessEvent"/>.
        /// </summary>
        /// <param name="userAccount">Instance of invited
        /// <see cref="UserAccount"/>.</param>
        /// <param name="invitedByUserAccount">Instance of
        /// <see cref="UserAccount"/> which created the invitation.</param>
        /// <exception cref="ArgumentNullException">Thrown when userAccount
        /// is null.</exception>
        public UserAccountInvitedSuccessEvent(
            UserAccount userAccount,
            UserAccount invitedByUserAccount)
            : base(EventCategories.UserAccount,
                  "UserAccount invited success",
                  EventTypes.Success,
                  EventIds.UserAccountInvited)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }
            
            this.UserAccountId = userAccount.Id;
            this.InvitedByUserAccountId = invitedByUserAccount?.Id; 
        }

        /// <summary>
        /// User accounts primary key.
        /// </summary>
        public Guid UserAccountId { get; set; }

        /// <summary>
        /// User acccounts who invited primary key.
        /// </summary>
        public Guid? InvitedByUserAccountId { get; set; }
    }

    public static partial class IEventServiceExtensions
    {
        /// <summary>
        /// Raises <see cref="UserAccountUpdatedSuccessEvent"/>.
        /// </summary>
        /// <param name="eventService">Instance of
        /// <see cref="IEventService"/>.</param>        
        /// <param name="userAccount">Instance of updated
        /// <see cref="UserAccount"/>.</param>
        /// <param name="invitedUserAccount">Instance of
        /// <see cref="UserAccount"/> which created the invitation.</param>
        public static async Task RaiseUserAccountInvitedSuccessEventAsync(
            this IEventService eventService,
            UserAccount userAccount,
            UserAccount invitedUserAccount)
        {
            await eventService.RaiseAsync(
                new UserAccountInvitedSuccessEvent(
                    userAccount, invitedUserAccount)
                );
        }
    }

}
