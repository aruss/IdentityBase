// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using IdentityBase.Models;
    using IdentityServer4.Models;

    public class IdentityBaseContext
    {
        /// <summary>
        /// Indicates if <see cref="IdentityBaseContext"/> is valid.
        /// </summary>
        public bool IsValid
        {
            get
            {
                return this.Client != null;
            }
        }

        public string ReturnUrl { get; set; }
        public AuthorizationRequest AuthorizationRequest { get; set; }

        public string LogoutId { get; set; }
        public LogoutRequest LogoutRequest { get; set; }

        /// <summary>
        /// Veryfication key is used for change email, reset password features
        /// eg /recover/confirm?verificationKey=asd5ds4gf65dsf4gd65s4sd...
        /// </summary>
        public string VerificationKey { get; set; }

        /// <summary>
        /// Current context client 
        /// </summary>
        public Client Client { get; set; }

        public ClientProperties ClientProperties { get; set; }
    }
}