// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    public static partial class Constants
    {
        public const string AuthenticationTypePassword = "password";
    }

    public static class EmailTemplates
    {
        public const string UserAccountCreated = "UserAccountCreated";
        public const string UserAccountRecover = "UserAccountRecover";
        public const string UserAccountInvited = "UserAccountInvited";
        public const string UserAccountEmailChanged = "UserAccountEmailChanged";
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

        public const string EmailAddressAlreadyTaken =
            "Email address is already taken.";

        public const string InvalidCredentials =
            "Invalid username or password.";

        public const string AccountIsDesabled =
            "Your user account has be disabled";

        public const string ConfirmAccount =
            "Please confirm your email account";

        public static string AccountAlreadyExists =
            "User already exists"; 
    }
}
