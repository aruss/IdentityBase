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
            ICrypto crypto,
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

                    PasswordHash  = crypto.HashPassword(
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

                    PasswordHash  = crypto.HashPassword(
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

                    PasswordHash  = crypto.HashPassword(
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
                    PasswordHash  = crypto.HashPassword(
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
                ///////////////////////////////////////////
                // Console Client Credentials Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = {
                        "api1",
                        "api2.read_only",
                        "idbase"
                    }
                },

                ///////////////////////////////////////////
                // Console Client Credentials Flow with client JWT assertion
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client.jwt",
                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants
                                .SecretTypes.X509CertificateBase64,

                            Value = "MIIDATCCAe2gAwIBAgIQoHUYAquk9rBJcq8W+F0FAzAJBgUrDgMCHQUAMBIxEDAOBgNVBAMTB0RldlJvb3QwHhcNMTAwMTIwMjMwMDAwWhcNMjAwMTIwMjMwMDAwWjARMQ8wDQYDVQQDEwZDbGllbnQwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDSaY4x1eXqjHF1iXQcF3pbFrIbmNw19w/IdOQxbavmuPbhY7jX0IORu/GQiHjmhqWt8F4G7KGLhXLC1j7rXdDmxXRyVJBZBTEaSYukuX7zGeUXscdpgODLQVay/0hUGz54aDZPAhtBHaYbog+yH10sCXgV1Mxtzx3dGelA6pPwiAmXwFxjJ1HGsS/hdbt+vgXhdlzud3ZSfyI/TJAnFeKxsmbJUyqMfoBl1zFKG4MOvgHhBjekp+r8gYNGknMYu9JDFr1ue0wylaw9UwG8ZXAkYmYbn2wN/CpJl3gJgX42/9g87uLvtVAmz5L+rZQTlS1ibv54ScR2lcRpGQiQav/LAgMBAAGjXDBaMBMGA1UdJQQMMAoGCCsGAQUFBwMCMEMGA1UdAQQ8MDqAENIWANpX5DZ3bX3WvoDfy0GhFDASMRAwDgYDVQQDEwdEZXZSb290ghAsWTt7E82DjU1E1p427Qj2MAkGBSsOAwIdBQADggEBADLje0qbqGVPaZHINLn+WSM2czZk0b5NG80btp7arjgDYoWBIe2TSOkkApTRhLPfmZTsaiI3Ro/64q+Dk3z3Kt7w+grHqu5nYhsn7xQFAQUf3y2KcJnRdIEk0jrLM4vgIzYdXsoC6YO+9QnlkNqcN36Y8IpSVSTda6gRKvGXiAhu42e2Qey/WNMFOL+YzMXGt/nDHL/qRKsuXBOarIb++43DV3YnxGTx22llhOnPpuZ9/gnNY7KLjODaiEciKhaKqt/b57mTEz4jTF4kIg6BP03MUfDXeVlM1Qf1jB43G2QQ19n5lUiqTpmQkcfLfyci2uBZ8BkOhXr3Vk9HIk/xBXQ="
                        }
                    },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = {
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // Custom Grant Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client.custom",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = {
                        "custom",
                        "custom.nosubject"
                    },
                    AllowedScopes = {
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // Console Resource Owner Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "custom.profile",
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // Console Public Resource Owner Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // Console Hybrid with PKCE Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "console.hybrid.pkce",
                    ClientName = "Console Hybrid with PKCE Sample",
                    RequireClientSecret = false,

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    RequirePkce = true,

                    RedirectUris = { "http://127.0.0.1" },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // Introspection Client Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient.reference",
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = {
                        "api1",
                        "api2.read_only"
                    },

                    AccessTokenType = AccessTokenType.Reference
                },

                ///////////////////////////////////////////
                // MVC Implicit Flow Samples
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.implicit",
                    ClientName = "MVC Implicit",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris =  {
                        "http://localhost:44077/signin-oidc"
                    },

                    FrontChannelLogoutUri =
                        "http://localhost:44077/signout-oidc",

                    PostLogoutRedirectUris = {
                        "http://localhost:44077/signout-callback-oidc"
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // MVC Manual Implicit Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.manual",
                    ClientName = "MVC Manual",
                    ClientUri = "http://identityserver.io",

                    AllowedGrantTypes = GrantTypes.Implicit,

                    RedirectUris = {
                        "http://localhost:44078/home/callback"
                    },

                    FrontChannelLogoutUri =
                        "http://localhost:44078/signout-oidc",

                    PostLogoutRedirectUris = {
                        "http://localhost:44078/"
                    },

                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId
                    }
                },

                ///////////////////////////////////////////
                // MVC Hybrid Flow Samples
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mvc.hybrid",
                    ClientName = "MVC Hybrid",
                    ClientUri = "http://localhost:21402",
                    //LogoUri = "https://pbs.twimg.com/profile_images/1612989113/Ki-hanja_400x400.png",

                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.Hybrid,
                    AllowAccessTokensViaBrowser = false,

                    RedirectUris = {
                        "http://localhost:21402/signin-oidc"
                    },

                    FrontChannelLogoutUri =
                        "http://localhost:21402/signout-oidc",

                    PostLogoutRedirectUris = {
                        "http://localhost:21402/signout-callback-oidc"
                    },

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "api2.read_only",
                        "idbase"
                    }
                },

                ///////////////////////////////////////////
                // JS PhoneGap Sample
                //////////////////////////////////////////
                new Client
                { 
                    ClientId = "phonegapclient",
                    ClientName = "PhoneGap Client",
                    AccessTokenType = AccessTokenType.Reference,
                    AccessTokenLifetime = 330,
                    IdentityTokenLifetime = 300,
                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = new List<string>
                    {
                        "http://localhost:3000"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:3000"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:3000"
                    },
                    AllowedScopes = new List<string>
                    {
                        "openid",
                        "role",
                        "profile",
                        "email",
                        "api1",
                        "api2.read_only"
                    }
                },

                ///////////////////////////////////////////
                // JS OAuth 2.0 Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "js_oauth",
                    ClientName = "JavaScript OAuth 2.0 Client",
                    ClientUri = "http://identityserver.io",
                    //LogoUri = "https://pbs.twimg.com/profile_images/1612989113/Ki-hanja_400x400.png",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,

                    RedirectUris = {
                        "http://localhost:28895/index.html"
                    },
                    AllowedScopes = {
                        "api1",
                        "api2.read_only"
                    }
                },
                
                ///////////////////////////////////////////
                // JS OIDC Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "js_oidc",
                    ClientName = "JavaScript OIDC Client",
                    ClientUri = "http://identityserver.io",
                    //LogoUri = "https://pbs.twimg.com/profile_images/1612989113/Ki-hanja_400x400.png",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireClientSecret = false,
                    AccessTokenType = AccessTokenType.Jwt,

                    RedirectUris =
                    {
                        "http://localhost:7017/index.html",
                        "http://localhost:7017/callback.html",
                        "http://localhost:7017/silent.html",
                        "http://localhost:7017/popup.html"
                    },

                    PostLogoutRedirectUris = {
                        "http://localhost:7017/index.html"
                    },

                    AllowedCorsOrigins = {
                        "http://localhost:7017"
                    },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "api1",
                        "api2.read_only",
                        "api2.full_access"
                    }
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

                // custom identity resource with some consolidated claims
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
                    ApiSecrets = { new Secret("secret".Sha256()) }
                },

                // expanded version if more control is needed
                new ApiResource
                {
                    Name = "api2",

                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    UserClaims =
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email
                    },

                    Scopes =
                    {
                        new Scope()
                        {
                            Name = "api2.full_access",
                            DisplayName = "Full access to API 2",
                        },
                        new Scope
                        {
                            Name = "api2.read_only",
                            DisplayName = "Read only access to API 2"
                        }
                    }
                },

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