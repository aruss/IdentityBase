using System.IO;
using IdentityBase.Public;

namespace CustomTheme
{
    /// <summary>
    /// The thing you always need
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Where the magic happens 
        /// </summary>
        /// <param name="args">Command line args</param>
        public static void Main(string[] args)
        {
            IdentityBaseWebHost
                .Run<Startup>(args, Directory.GetCurrentDirectory());

            // In case you want to generate a password hash
            // var password = new IdentityBase.Crypto.DefaultCrypto()
            //     .HashPassword("UweMachtsMöglich14", 0);
            // System.Console.WriteLine(password); 
        }
    }
}
