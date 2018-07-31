/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 *
 * Code borrowed from: https://github.com/brockallen/BrockAllen.MembershipReboot/tree/master/src/BrockAllen.MembershipReboot/Crypto
 */

namespace IdentityBase.Crypto
{
    public interface ICryptoService
    {
        string HashPassword(
            string password,
            int iterations);

        bool VerifyPasswordHash(
            string hashedPassword,
            string password,
            int iterations);

        string GenerateSalt();
        string Hash(string value);
    }
}
