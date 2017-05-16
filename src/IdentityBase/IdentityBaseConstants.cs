using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase
{
    public static partial class IdentityBaseConstants
    {
        public const string AuthenticationTypePassword = "password";

        public static class Routes
        {
            public const string Register = "register";
            public const string RegisterSuccess = "register/success";
        }

        public static class EmailTemplates
        {
            public const string UserAccountCreated = "UserAccountCreated";
            public const string UserAccountRecover = "UserAccountRecover";
        }
    }
}
