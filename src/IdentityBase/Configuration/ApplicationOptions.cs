// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Configuration
{
    /// <summary>
    /// IdentityBase application options
    /// </summary>
    public class ApplicationOptions
    {
        // TODO: Find another way to get the current public URI, while application start.
        public string PublicUrl { get; set; } = "http://localhost:5000";

        public string TempFolder { get; set; } = "./AppData/Temp";
        
        // Local account options

        public int PasswordHashingIterationCount { get; set; } = 0;
        public int AccountLockoutFailedLoginAttempts { get; set; } = 5;

        /// <summary>
        /// Duration for account lockout in seconds
        /// </summary>
        public int AccountLockoutDuration { get; set; } = 600; // 10 minutes

        /// <summary>
        /// If enabled users have to confirm the registration by clicking in
        /// the link in the mail in order to login.
        /// </summary>
        public bool RequireLocalAccountVerification { get; set; } = true;

        // External account options

        /// <summary>
        /// If enabled users have to confirm the registration if the come from
        /// third party IDP
        /// </summary>
        public bool RequireExternalAccountVerification { get; set; } = false;

        // Common options

        /// <summary>
        /// If enabled user may delete his own account
        /// </summary>
        public bool EnableAccountDeletion { get; set; } = false;

        /// <summary>
        /// If user has trouble with login IdSrv will show all possible
        /// accounts which are
        /// </summary>
        public bool EnableLoginHints { get; set; } = false;

        /// <summary>
        /// Login user automatically after he created a local account
        /// </summary>
        public bool LoginAfterAccountCreation { get; set; } = true;

        /// <summary>
        /// Login user automatically after account confirmation
        /// </summary>
        public bool LoginAfterAccountConfirmation { get; set; } = true;

        /// <summary>
        /// Login user automatically after successful recovery
        /// </summary>
        public bool LoginAfterAccountRecovery { get; set; } = true;

        /// <summary>
        /// Timespan the confirmation and concelation links a valid in seconds
        /// </summary>
        public int VerificationKeyLifetime { get; set; } = 86400; // 24 hours

        public bool EnableAccountRegistration { get; set; } = true;

        public bool EnableAccountRecovery { get; set; } = true;

        /// <summary>
        /// Automatically merges third party accounts with local account if
        /// email matches.
        /// </summary>
        public bool AutomaticAccountMerge { get; set; } = true;

        public bool EnableRememberLogin { get; set; } = true;

        public bool EnableLocalLogin { get; set; } = true;

        /// <summary>
        /// How long should Remember Login last in seconds.
        /// </summary>
        public double RememberMeLoginDuration { get; set; } = 28800; // 8 hours

        public bool ShowLogoutPrompt { get; set; } = false;

        public bool AutomaticRedirectAfterSignOut { get; set; } = true;

        public bool WindowsAuthenticationEnabled { get; set; } = false;

        // specify the Windows authentication schemes you want to use for authentication
        public string[] WindowsAuthenticationSchemes { get; set; } =
            new string[] { "Negotiate", "NTLM" };

        public string WindowsAuthenticationDisplayName { get; set; } =
            "Windows";

        public string ThemePath { get; set; } = "./Themes/Default";

        /// <summary>
        /// Enables invitation get API 
        /// </summary>
        public bool EnableInvitationGetEndpoint { get; set; } = false;

        /// <summary>
        /// Enables invitation create API 
        /// </summary>
        public bool EnableInvitationCreateEndpoint { get; set; } = false;

        /// <summary>
        /// Enbales invitation delete API 
        /// </Enables>
        public bool EnableInvitationDeleteEndpoint { get; set; } = false;
        
        /// <summary>
        /// Enbales user account delete API 
        /// </Enables>
        public bool EnableUserAccountDeleteEndpoint { get; set; } = false;

        /// <summary>
        /// Enables email change API 
        /// </summary>
        public bool EnableAccountChangeEmailEndpoint { get; set; } = false;

        /// <summary>
        /// In case idbase API is active, this will be used as secret for
        /// authentication
        /// </summary>
        public string ApiSecret { get; set; } = "secret";
    }
}
