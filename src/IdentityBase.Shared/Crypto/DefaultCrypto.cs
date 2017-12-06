/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 *
 * Code borrowed from: https://github.com/brockallen/BrockAllen.MembershipReboot/tree/master/src/BrockAllen.MembershipReboot/Crypto
 */
 
namespace IdentityBase.Crypto
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;

    public interface ICrypto
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

    public class DefaultCrypto : ICrypto
    {
        public const char PasswordHashingIterationCountSeparator = '.';

        public string HashPassword(string password, int iterations)
        {
            if (iterations <= 0)
            {
                iterations = GetIterationsFromYear(GetCurrentYear());
            }

            string result = HashPasswordInternal(password, iterations);

            return EncodeIterations(iterations) +
                PasswordHashingIterationCountSeparator +
                result;
        }

        public bool VerifyPasswordHash(
            string hashedPassword,
            string password,
            int iterations)
        {
            if (!hashedPassword.Contains(
                DefaultCrypto.PasswordHashingIterationCountSeparator))
            {
                return this.VerifyPasswordHashInternal(
                   hashedPassword,
                   password,
                   PBKDF2IterCount
               );
            }

            string[] parts = hashedPassword
                .Split(PasswordHashingIterationCountSeparator);

            if (parts.Length != 2)
            {
                return false;
            }

            int count = DecodeIterations(parts[0]);
            if (count <= 0)
            {
                return false;
            }

            hashedPassword = parts[1];

            return this.VerifyPasswordHashInternal(
                hashedPassword,
                password,
                count
            );
        }

        // From OWASP : https://www.owasp.org/index.php/Password_Storage_Cheat_Sheet
        public const int StartYear = 2016;
        public const int StartCount = 1000;

        public int GetIterationsFromYear(int year)
        {
            if (year <= StartYear)
            {
                return StartCount;
            }

            int diff = (year - StartYear) / 2;
            int mul = (int)Math.Pow(2, diff);
            int count = StartCount * mul;

            // if we go negative, then we wrapped (expected in year ~2044).
            // Int32.Max is best we can do at this point
            if (count < 0)
            {
                count = Int32.MaxValue;
            }

            return count;
        }

        public virtual int GetCurrentYear()
        {
            return DateTime.Now.Year;
        }

        public string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        public const int PBKDF2IterCount = 1000; // default for Rfc2898DeriveBytes
        public const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
        public const int SaltSize = 128 / 8; // 128 bits

        public string HashPasswordInternal(string password, int iterations)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Produce a version 0 (see comment above) password hash.
            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(
                password, SaltSize, iterations))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            byte[] outputBytes = new byte[1 + SaltSize + PBKDF2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);

            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize,
                PBKDF2SubkeyLength);

            return Convert.ToBase64String(outputBytes);
        }

        public bool VerifyPasswordHashInternal(
            string passwordHash,
            string password,
            int iterationCount)
        {
            if (passwordHash == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] hashedPasswordBytes = Convert.FromBase64String(passwordHash);

            // Verify a version 0 (see comment above) password hash.
            if (hashedPasswordBytes.Length !=
                (1 + SaltSize + PBKDF2SubkeyLength) ||
                hashedPasswordBytes[0] != 0x00)
            {
                // Wrong length or version header.
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
            byte[] storedSubkey = new byte[PBKDF2SubkeyLength];

            Buffer.BlockCopy(
                hashedPasswordBytes,
                1 + SaltSize,
                storedSubkey,
                0,
                PBKDF2SubkeyLength
            );

            byte[] generatedSubkey;
            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(
                password,
                salt,
                iterationCount)
            )
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }

        public int DecodeIterations(string prefix)
        {
            int val;

            if (Int32.TryParse(
                prefix,
                System.Globalization.NumberStyles.HexNumber,
                null,
                out val)
            )
            {
                return val;
            }

            return -1;
        }

        /// <summary>
        /// Compares two byte arrays for equality. The method is specifically
        /// written so that the loop is not optimized. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        public string GenerateSalt()
        {
            var buf = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buf);
            }

            return Convert.ToBase64String(buf);
        }

        public string Hash(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return Hash(Encoding.UTF8.GetBytes(input));
        }

        public string Hash(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            using (HashAlgorithm alg = SHA256.Create())
            {
                if (alg == null)
                {
                    //String.Format(CultureInfo.InvariantCulture, HelpersResources.Crypto_NotSupportedHashAlg, algorithm));
                    throw new InvalidOperationException();

                }

                byte[] hashData = alg.ComputeHash(input);
                return this.BinaryToHex(hashData);
            }
        }

        internal string BinaryToHex(byte[] data)
        {
            char[] hex = new char[data.Length * 2];

            for (int iter = 0; iter < data.Length; iter++)
            {
                byte hexChar = ((byte)(data[iter] >> 4));

                hex[iter * 2] = (char)(hexChar > 9 ?
                    hexChar + 0x37 :
                    hexChar + 0x30);

                hexChar = ((byte)(data[iter] & 0xF));

                hex[(iter * 2) + 1] = (char)(hexChar > 9 ?
                    hexChar + 0x37 :
                    hexChar + 0x30);
            }
            return new string(hex);
        }
    }
}
