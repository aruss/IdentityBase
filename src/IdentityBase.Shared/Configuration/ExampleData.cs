// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using IdentityBase.Crypto;
    using IdentityBase.Models;
    using IdentityModel;
    using IdentityServer4;
    using IdentityServer4.Models;

    public class ExampleData
    {
        private IEnumerable<UserAccountClaim> CreateClaims(
            string name,
            string givenName,
            string familyName)
        {
            return new List<UserAccountClaim>
            {
                new UserAccountClaim(JwtClaimTypes.Name, name),
                new UserAccountClaim(JwtClaimTypes.GivenName, givenName),
                new UserAccountClaim(JwtClaimTypes.FamilyName, familyName)
            };
        }

        public IEnumerable<UserAccount> GetUserAccounts(
            ICryptoService cryptoService,
            ApplicationOptions options)
        {
            var now = DateTime.UtcNow;

            var users = new List<UserAccount>
            {
                // Active user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef"),
                    Email = "alice@localhost",

                    PasswordHash  = cryptoService.HashPassword(
                        "alice@localhost",
                        options.PasswordHashingIterationCount),

                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    IsLoginAllowed = true,

                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim(JwtClaimTypes.Name,
                            "Alice Smith"),

                        new UserAccountClaim(JwtClaimTypes.GivenName,
                            "Alice"),

                        new UserAccountClaim(JwtClaimTypes.FamilyName,
                            "Smith"),

                        new UserAccountClaim(JwtClaimTypes.Email,
                            "alice@localhost"),

                        new UserAccountClaim(JwtClaimTypes.EmailVerified,
                            "true",
                            ClaimValueTypes.Boolean),

                        new UserAccountClaim(JwtClaimTypes.Role, "Admin"),

                        new UserAccountClaim(JwtClaimTypes.Role, "Geek"),

                        new UserAccountClaim(JwtClaimTypes.WebSite,
                            "http://alice.com"),

                        new UserAccountClaim(JwtClaimTypes.Address,
                            @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                // Active user account 
                new UserAccount
                {
                    Id = Guid.Parse("28575826-68a0-4a1d-9428-674a2eb5db95"),
                    Email = "bob@localhost",

                    PasswordHash  = cryptoService.HashPassword(
                        "bob@localhost",
                        options.PasswordHashingIterationCount),

                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    IsLoginAllowed = true,

                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim(JwtClaimTypes.Name,
                            "Bob Smith"),

                        new UserAccountClaim(JwtClaimTypes.GivenName,
                            "Bob"),

                        new UserAccountClaim(JwtClaimTypes.FamilyName,
                            "Smith"),

                        new UserAccountClaim(JwtClaimTypes.Email,
                            "bob@localhost"),

                        new UserAccountClaim(JwtClaimTypes.EmailVerified,
                            "true",
                            ClaimValueTypes.Boolean),

                        new UserAccountClaim(JwtClaimTypes.Role,
                            "Developer"),

                        new UserAccountClaim(JwtClaimTypes.Role,
                            "Geek"),

                        new UserAccountClaim(JwtClaimTypes.WebSite,
                            "http://bob.com"),

                        new UserAccountClaim(JwtClaimTypes.Address,
                            @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }",
                            IdentityServerConstants.ClaimValueTypes.Json)
                    }
                },
                
                // Inactive user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("6b13d17c-55a6-482e-96b9-dc784015f927"),
                    Email = "jim@localhost",

                    PasswordHash  = cryptoService.HashPassword(
                        "jim@localhost",
                        options.PasswordHashingIterationCount),

                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    EmailVerifiedAt = now,
                    IsLoginAllowed = false,
                    Claims = CreateClaims("Jim Panse", "Jim", "Panse"),
                },

                // Not verified user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("13808d08-b1c0-4f28-8d3e-8c9a4051efcb"),
                    Email = "paul@localhost",
                    PasswordHash  = cryptoService.HashPassword(
                        "paul@localhost",
                        options.PasswordHashingIterationCount),

                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = false,
                    IsLoginAllowed = false,
                    Claims = CreateClaims("Paul Panzer", "Paul", "Panzer")
                    // TODO: set VerificationKey, VerificationPurpose, VerificationKeySentAt
                },

                // External user account
                new UserAccount
                {
                    Id = Guid.Parse("58631b04-9be5-454a-aa1d-f679cd454fa6"),
                    Email = "bill@localhost",
                    CreatedAt = now,
                    UpdatedAt = now,
                    // had never confirmed the email, since he got via facebook
                    IsEmailVerified = false,
                    // is allowed to login since he registed via facebook
                    IsLoginAllowed = true,  
                    Claims = CreateClaims("Bill Smith", "Bill", "Smith"),
                    Accounts = new List<ExternalAccount>()
                    {
                        new ExternalAccount
                        {
                            CreatedAt = now,
                            Email = "bill@localhost",
                            Subject = "123456789",
                            Provider = "facebook"
                        }
                    }
                }
            };

            return users;
        }

        public IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                // client credentials client
           
                new Client
                {
                    ClientId = "client",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                },

                // resource owner password grant client
                new Client
                {
                    ClientId = "ro.client",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "api1" }
                },

                // OpenID Connect hybrid flow and client credentials client (MVC)
                new Client
                {
                    ClientId = "mvc",
                    ClientName = "MVC Client",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                    RequireConsent = true,

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    RedirectUris = { "http://localhost:5002/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "idbase"
                    },
                    AllowOfflineAccess = true
                }
            };        
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new[]
            {
                // some standard scopes from the OIDC spec
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),

                // Custom identity resource with some consolidated claims
                new IdentityResource("custom.profile", new[] {
                    JwtClaimTypes.Name,
                    JwtClaimTypes.Email,
                    "location"
                })
            };
        }

        public IEnumerable<ApiResource> GetApiResources()
        {
            return new[]
            {
                // simple version with ctor
                new ApiResource("api1", "Some API 1")
                {
                    // this is needed for introspection when using reference tokens
                    ApiSecrets = {
                        new Secret("secret".Sha256())
                    }
                },

                // IdentityBase API for invitation and other features
                new ApiResource
                {
                    Name = "idbase",
                    DisplayName = "IdentityBase",

                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    Scopes =
                    {
                        new Scope()
                        {
                            Name = "idbase",
                            DisplayName =
                                "Full access to IdentityBase API",
                        }                        
                    }
                }
            };
        }
    }
}