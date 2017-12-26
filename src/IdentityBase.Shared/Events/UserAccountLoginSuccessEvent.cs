// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Events
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using ServiceBase.Events;

    /// <summary>
    /// Event for successfull <see cref="UserAccount"/> Login.
    /// </summary>
    public class UserAccountLoginSuccessEvent : Event
    {
        /// <summary>
        /// Created a instace of <see cref="UserAccountLoginSuccessEvent"/>.
        /// </summary>
        /// <param name="userAccount">Instance of logged in
        /// <see cref="UserAccount"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when
        /// invitedUserAccount is null.</exception>
        public UserAccountLoginSuccessEvent(
            UserAccount userAccount)
            : base(EventCategories.UserAccount,
                  "UserAccount login success",
                  EventTypes.Success,
                  EventIds.UserAccountLogin)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            this.UserAccountId = userAccount.Id; 
        }

        /// <summary>
        /// User accounts primary key.
        /// </summary>
        public Guid UserAccountId { get; set; }
    }

    public static partial class IEventServiceExtensions
    {
        /// <summary>
        /// Raises <see cref="UserAccountLoginSuccessEvent"/>.
        /// </summary>
        /// <param name="eventService">Instance of
        /// <see cref="IEventService"/>.</param>
        /// <param name="userAccount">Instance of updated
        /// <see cref="UserAccount"/>.</param>
        public static async Task RaiseUserAccountLoginSuccessEventAsync(
            this IEventService eventService,
            UserAccount userAccount)
        {
            await eventService.RaiseAsync(
                new UserAccountLoginSuccessEvent(userAccount));
        }
    }
}
