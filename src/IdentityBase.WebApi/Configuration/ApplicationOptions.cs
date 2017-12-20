// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Configuration
{
    /// <summary>
    /// IdentityBase WebAPI options
    /// </summary>
    public class WebApiOptions
    {
        /// <summary>
        /// In case idbase API is active, this will be used as secret for
        /// authentication.
        /// </summary>
        public string ApiSecret { get; set; } = "secret";

        /// <summary>
        /// Base-address of the token issuer
        /// </summary>
        public string AutorityUrl { get; set; } = "http://localhost:5000";

        public bool EnableSwagger { get; set; } = false; 
    }
}
