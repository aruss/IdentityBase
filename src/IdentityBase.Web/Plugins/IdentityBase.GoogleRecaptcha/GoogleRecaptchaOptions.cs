// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.GoogleRecaptcha
{
    public class GoogleRecaptchaOptions
    {
        /// <summary>
        /// Enables ReCAPTCHA prompt on account login form.
        /// </summary>
        public bool EnableOnLogin { get; set; } = true;

        /// <summary>
        /// Enables ReCAPTCHA prompt on account registration form.
        /// </summary>
        public bool EnableOnRegister { get; set; } = true;

        /// <summary>
        /// Enables ReCAPTCHA prompt on account recovery form.
        /// </summary>
        public bool EnableOnRecover { get; set; } = true; 

        public string SiteKey { get; set; }
        public string Secret { get; set; }
    }
}
