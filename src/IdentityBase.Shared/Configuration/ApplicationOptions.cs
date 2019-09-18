// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Configuration
{
    /// <summary>
    /// IdentityBase application options
    /// </summary>
    public class ApplicationOptions
    {
        /// <summary>
        /// Location of temporary folder.
        /// </summary>
        public string TempPath { get; set; } = "./temp";
        
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

        /// <summary>
        /// Enables local account registration and registration confirmation.
        /// </summary>
        public bool EnableAccountRegistration { get; set; } = true;

        /// <summary>
        /// Enables local account recovery and recovery confirmation.
        /// </summary>
        public bool EnableAccountRecovery { get; set; } = true;

        /// <summary>
        /// Enables local account invitation and invitation confirmation.
        /// </summary>
        public bool EnableAccountInvitation { get; set; } = true;

        /// <summary>
        /// Automatically merges third party accounts with local account if
        /// email matches.
        /// </summary>
        public bool AutomaticAccountMerge { get; set; } = true;
   
        /// <summary>
        /// Enables local account login form. 
        /// </summary>
        public bool EnableAccountLogin { get; set; } = true;

        /// <summary>
        /// Enables "Remember Me" option for login. 
        /// </summary>
        public bool EnableRememberMe { get; set; } = false;

        /// <summary>
        /// How long should Remember Login last in seconds.
        /// </summary>
        public double RememberMeDuration { get; set; } = 28800; // 8 hours

        public bool ShowLogoutPrompt { get; set; } = false;

        public bool AutomaticRedirectAfterSignOut { get; set; } = true;

        public bool WindowsAuthenticationEnabled { get; set; } = false;
        
        // specify the Windows authentication schemes you want to use for authentication
        public string[] WindowsAuthenticationSchemes { get; set; } =
            new string[] { "Negotiate", "NTLM" };

        public string WindowsAuthenticationDisplayName { get; set; } =
            "Windows";

        public string PluginsPath { get; set; } = "./Plugins"; 

        /// <summary>
        /// Default UI culture of IdentityServer.
        /// </summary>
        public string DefaultCulture { get; set; } = "de-DE"; // "en-US";

        /// <summary>
        /// If set to true IdentityServer won't reveal any existence of the
        /// user account, instead a generic message will be displayed and a
        /// mail with further information will be sent to the user account email.
        /// </summary>
        public bool ObfuscateUserAccountPresence { get; set; } = false;
    }
}
