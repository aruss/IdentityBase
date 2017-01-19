using System;

namespace ServiceBase.IdentityServer.Configuration
{
    /// <summary>
    /// IdentityBase application options
    /// </summary>
    public class ApplicationOptions
    {
        // Local account options

        public int PasswordHashingIterationCount { get; set; } = 0;
        public int AccountLockoutFailedLoginAttempts { get; set; } = 5;
        public int AccountLockoutDuration { get; set; } = 600; // 10 minutes

        /// <summary>
        /// If enabled users have to confirm the registration by clicking in the link in the mail in order to login.
        /// </summary>
        public bool RequireLocalAccountVerification { get; set; } = true;

        // External account options

        /// <summary>
        /// If enabled users have to confirm the registration if the come from third party IDP
        /// </summary>
        public bool RequireExternalAccountVerification { get; set; } = false;

        // Common options

        /// <summary>
        /// If enabled user may delete his own account
        /// </summary>
        public bool EnableAccountDeletion { get; set; } = false;

        /// <summary>
        /// If user has trouble with login IdSrv will show all possible accounts which are
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
        /// Timespan the confirmation and concelation links a valid in minutes
        /// </summary>
        public int VerificationKeyLifetime { get; set; } = 1440; // 24 hours

        /// <summary>
        /// Automatically merges third party accounts with local account if email matches
        /// </summary>
        public bool AutomaticAccountMerge { get; set; } = true;

        public bool EnableRememberLogin { get; set; } = true;

        /// <summary>
        /// How long should Remember Login last in days
        /// </summary>
        public int RememberMeLoginDuration { get; set; } = 30;

        public bool EnableLocalLogin { get; set; } = true;

        public bool ShowLogoutPrompt = false;
        public bool AutomaticRedirectAfterSignOut = true;
    }
}
