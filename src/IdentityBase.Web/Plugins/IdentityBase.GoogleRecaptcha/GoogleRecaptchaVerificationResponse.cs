// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Newtonsoft.Json;

namespace IdentityBase.GoogleRecaptcha
{
    public class GoogleRecaptchaVerificationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
        /// </summary>
        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTimestamp { get; set; }

        /// <summary>
        /// The hostname of the site where the reCAPTCHA was solved
        /// </summary>
        [JsonProperty("hostname")]
        public string HostName { get; set; }

        /// <summary>
        /// Error codes those are
        ///     missing-input-secret	The secret parameter is missing.
        ///     invalid-input-secret    The secret parameter is invalid or malformed.
        ///     missing-input-response  The response parameter is missing.
        ///     invalid-input-response  The response parameter is invalid or malformed.
        ///     bad-request             The request is invalid or malformed.
        /// </summary>
        [JsonProperty("error-codes")]
        public string[] ErrorCodes { get; set; }
    }

}
