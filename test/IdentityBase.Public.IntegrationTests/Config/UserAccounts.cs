using IdentityModel;
using IdentityServer4;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace IdentityBase.Public.IntegrationTests.Config
{
    public class UserAccounts
    {
        public static List<UserAccount> Get(ICrypto crypto, ApplicationOptions options)
        {
            var now = DateTime.UtcNow;

            var users = new List<UserAccount>
            {
                new UserAccount
                {
                    Id = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef"),
                    Email = "alice@localhost",
                    PasswordHash  = crypto.HashPassword("alice@localhost", options.PasswordHashingIterationCount),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    IsLoginAllowed = true,
                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim(JwtClaimTypes.Name, "Alice Smith") { Id = Guid.Parse("1338ad75-8453-4024-b5ff-657b1bc79a4c"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.GivenName, "Alice") { Id = Guid.Parse("a4342819-61c2-4ae0-8ce2-649a8ad89bd5"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.FamilyName, "Smith") { Id = Guid.Parse("a31f501c-6b77-466d-b1a0-3f92daef5e60"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.Email, "alice@localhost") { Id = Guid.Parse("a31f501c-6b77-466d-b1a0-3f92daef5e60"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean) { Id = Guid.Parse("ea7c1cd4-0346-44ce-8675-39b39d5c663d"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.Role, "Admin") { Id = Guid.Parse("a916297c-d8fb-455f-8da9-1ec00af90b71"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.Role, "Geek") { Id = Guid.Parse("0b6daa16-c51b-4510-b69b-3d402716a82a"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.WebSite, "http://alice.com") { Id = Guid.Parse("bb3d1029-973f-463c-92a7-4dab1a063453"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") },
                        new UserAccountClaim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json) { Id = Guid.Parse("3b28ab5d-2976-4802-a2e5-b468d323e5c4"), UserAccountId = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef") }
                    }
                },
                new UserAccount
                {
                    Id = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95"),
                    Email = "bob@localhost",
                    PasswordHash  = crypto.HashPassword("bob@localhost", options.PasswordHashingIterationCount),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    IsLoginAllowed = true,
                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim(JwtClaimTypes.Name, "Bob Smith") { Id = Guid.Parse("58389f05-ed6c-431b-8318-a530e55d5450"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.GivenName, "Bob") { Id = Guid.Parse("e7b6a0f6-5437-48ef-b868-a949991fadfe"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.FamilyName, "Smith") { Id = Guid.Parse("6398ac60-67ce-48e4-9803-de9c244dcd7b"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.Email, "bob@localhost") { Id = Guid.Parse("2ce9ee83-8fb6-4b41-bef1-0551e010fde6"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean) { Id = Guid.Parse("26c8d08b-ff04-43d0-b80e-4252f04241e3"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.Role, "Developer") { Id = Guid.Parse("cd6f4039-8fee-43fe-8058-48ffa680d5de"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.Role, "Geek") { Id = Guid.Parse("298d7a32-cc8f-4ec0-837f-aeed1ae43e74"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.WebSite, "http://bob.com") { Id = Guid.Parse("41e399c7-dec0-4696-be2b-0067f6b1c6a1"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") },
                        new UserAccountClaim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json) { Id = Guid.Parse("79d46959-3a46-4b26-8bf9-808c50651c90"), UserAccountId = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95") }
                    }
                }
            };
            
            return users;
        }
    }
}