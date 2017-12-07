// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Events
{
    using System;
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using ServiceBase.Events;

    /// <summary>
    /// Event for successful <see cref="UserAccount"/> creation.
    /// </summary>
    public class UserAccountCreatedSuccessEvent : Event
    {
        /// <summary>
        /// Created a instace of <see cref="UserAccountCreatedSuccessEvent"/>.
        /// </summary>
        /// <param name="userAccount">Instance of created
        /// <see cref="UserAccount"/>.</param>
        /// <param name="provider">Name of the identity provider
        /// caused the creation, e.g. local, facebook, google.</param>
        /// <exception cref="ArgumentNullException">Thrown when userAccount
        /// is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when provider is
        /// null or empty.</exception>
        public UserAccountCreatedSuccessEvent(
            UserAccount userAccount,
            string provider)
            : base(EventCategories.UserAccount,
                  "UserAccount created success",
                  EventTypes.Success,
                  EventIds.UserAccountCreated)
        {
            if (userAccount == null)
            {
                throw new ArgumentNullException(nameof(userAccount));
            }

            if (String.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentNullException(nameof(provider));
            }
            
            this.UserAccountId = userAccount.Id;
            this.Provider = provider;
        }

        /// <summary>
        /// User accounts primary key.
        /// </summary>
        public Guid UserAccountId { get; set; }

        /// <summary>
        /// Used Identity Provider.
        /// </summary>
        public string Provider { get; set; }
    }

    public static partial class IEventServiceExtensions
    {
        /// <summary>
        /// Raises <see cref="UserAccountCreatedSuccessEvent"/>.
        /// </summary>
        /// <param name="eventService">Instance of
        /// <see cref="IEventService"/>.</param>
        /// <param name="userAccount">Instance of created
        /// <see cref="UserAccount"/>.</param>
        /// <param name="provider">Name of the identity provider
        /// caused the creation, e.g. local, facebook, google.</param>       
        public static async Task RaiseUserAccountCreatedSuccessEventAsync(
            this IEventService eventService,
            UserAccount userAccount,
            string provider)
        {
            await eventService.RaiseAsync(
                new UserAccountCreatedSuccessEvent(userAccount, provider));
        }
    }
}
