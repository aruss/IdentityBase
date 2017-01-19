using System;

namespace ServiceBase.IdentityServer.Extensions
{
    public static class StringExtensions
    {
        static readonly string[] UglyBase64 = { "+", "/", "=" };
        public static string StripUglyBase64(this string s)
        {
            if (String.IsNullOrWhiteSpace(s))
            {
                return s;
            }
            
            foreach (var ugly in UglyBase64)
            {
                s = s.Replace(ugly, String.Empty);
            }

            return s;
        }
    }
}
