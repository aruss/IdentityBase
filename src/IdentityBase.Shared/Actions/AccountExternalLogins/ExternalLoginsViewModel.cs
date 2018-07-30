// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Account
{
    using System.Collections.Generic;
    using IdentityBase.Models;

    public class ExternalLoginsViewModel
    {
        /// <summary>
        /// Providers the user account is assosiated with.
        /// </summary>
        public IEnumerable<ExternalProvider> ConnectedProviders { get; set; }

        /// <summary>
        /// Accounts available to be connected.
        /// </summary>
        public IEnumerable<ExternalProvider> AvailableProviders { get; set; }

        public string ClientId { get; set; }
    }
}
