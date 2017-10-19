// [assembly: UserSecretsId("IdentityBase.Public")]

namespace IdentityBase.Public
{
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            IdentityBaseWebHost.Run(args);
        }
    }
}