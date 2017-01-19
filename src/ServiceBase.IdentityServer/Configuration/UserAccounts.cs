using IdentityModel;
using IdentityServer4;
using ServiceBase.IdentityServer.Configuration;
using ServiceBase.IdentityServer.Crypto;
using ServiceBase.IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;

namespace ServiceBase.IdentityServer.Configuration
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
                        new UserAccountClaim(JwtClaimTypes.Name, "Alice Smith"),
                        new UserAccountClaim(JwtClaimTypes.GivenName, "Alice"),
                        new UserAccountClaim(JwtClaimTypes.FamilyName, "Smith"),
                        new UserAccountClaim(JwtClaimTypes.Email, "alice@localhost"),
                        new UserAccountClaim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new UserAccountClaim(JwtClaimTypes.Role, "Admin"),
                        new UserAccountClaim(JwtClaimTypes.Role, "Geek"),
                        new UserAccountClaim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new UserAccountClaim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json)
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
                        new UserAccountClaim(JwtClaimTypes.Name, "Bob Smith"),
                        new UserAccountClaim(JwtClaimTypes.GivenName, "Bob"),
                        new UserAccountClaim(JwtClaimTypes.FamilyName, "Smith"),
                        new UserAccountClaim(JwtClaimTypes.Email, "bob@localhost"),
                        new UserAccountClaim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new UserAccountClaim(JwtClaimTypes.Role, "Developer"),
                        new UserAccountClaim(JwtClaimTypes.Role, "Geek"),
                        new UserAccountClaim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new UserAccountClaim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json)
                    }
                }
            };

            return users;
        }
    }
}