namespace IdentityBase.Events
{
    public static class EventConstants
    {
        public static class Categories
        {
            public const string UserAccount = "UserAccount";
        }

        public static class Ids
        {
            public const int UserAccountCreated = 1000;
            public const int UserAccountUpdated = 1100;
            public const int UserAccountDeleted = 1200;
            public const int UserAccountInvited = 1400;
        }
    }
}
