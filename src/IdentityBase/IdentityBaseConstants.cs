namespace IdentityBase
{
    public static partial class IdentityBaseConstants
    {
        public const string AuthenticationTypePassword = "password";

        public const int TakeDefault = 10;
        public const int TakeMin = 1;
        public const int TakeMax = 1000;

        public const int SkipDefault = 0;
        public const int SkipMin = 0;
        public const int SkipMax = 999;

        public static class EmailTemplates
        {
            public const string UserAccountCreated = "UserAccountCreated";
            public const string UserAccountRecover = "UserAccountRecover";
            public const string UserAccountInvited = "UserAccountInvited";
        }

        public static class ErrorMessages
        {
            public const string UserAccountIsDeactivated =
                "User account is diactivated.";

            public const string UserAccountDoesNotExists =
                "User account does not exists.";

            public const string InvalidEmailAddress =
                "The Email field is not a valid e-mail address.";

            public const string TokenIsInvalid = "Invalid token.";

            public const string RecoveryNoReturnUrl =
                "Recovery attempt with missing ReturnUrl parameter.";
        }
    }
}
