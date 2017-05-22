using IdentityModel;
using Microsoft.Extensions.Options;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Public.EntityFramework.DbContexts;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Models;
using IdentityBase.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using IdentityBase.Public.IntegrationTests.Config;

namespace IdentityBase.Public.IntegrationTests
{
    public class TestStoreInitializer : IStoreInitializer, IDisposable
    {
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly PersistedGrantDbContext _persistedGrantDbContext;
        private readonly UserAccountDbContext _defaultDbContext;
        private readonly ICrypto _crypto;
        private readonly ApplicationOptions _applicationOptions;

        public TestStoreInitializer(
            ConfigurationDbContext configurationDbContext,
            PersistedGrantDbContext persistedGrantDbContext,
            UserAccountDbContext defaultDbContext,
            ICrypto crypto,
            ApplicationOptions applicationOptions)
        {
            _configurationDbContext = configurationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _defaultDbContext = defaultDbContext;
            _crypto = crypto;
            _applicationOptions = applicationOptions;
        }

        public void Dispose()
        {
        }

        private List<UserAccountClaim> CreateClaims(string name, string givenName, string familyName)
        {
            return new List<UserAccountClaim>
            {
                new UserAccountClaim(JwtClaimTypes.Name, name),
                new UserAccountClaim(JwtClaimTypes.GivenName, givenName),
                new UserAccountClaim(JwtClaimTypes.FamilyName, familyName)
            };
        }

        public void InitializeStores()
        {
            // Cleanup
            // HACK: the memory db survives server dispose so i have to cleanup it here
            //       cannot run this tests in parallel
            _configurationDbContext.Clients.Clear();
            _configurationDbContext.IdentityResources.Clear();
            _configurationDbContext.ApiResources.Clear();
            _configurationDbContext.SaveChanges();
            _persistedGrantDbContext.PersistedGrants.Clear();
            _persistedGrantDbContext.SaveChanges();
            _defaultDbContext.UserAccounts.Clear();
            _defaultDbContext.ExternalAccounts.Clear();
            _defaultDbContext.UserAccountClaims.Clear();
            _defaultDbContext.SaveChanges();

            // Add default sample data clients
            foreach (var client in Clients.Get().ToList())
            {
                _configurationDbContext.Clients.Add(client.ToEntity());
            }
            _configurationDbContext.SaveChanges();

            foreach (var resource in Resources.GetIdentityResources().ToList())
            {
                _configurationDbContext.IdentityResources.Add(resource.ToEntity());
            }
            _configurationDbContext.SaveChanges();

            foreach (var resource in Resources.GetApiResources().ToList())
            {
                _configurationDbContext.ApiResources.Add(resource.ToEntity());
            }
            _configurationDbContext.SaveChanges();

            // Add test user accounts
            var now = DateTime.UtcNow;
            var userAccounts = new List<UserAccount>
            {
                // Active user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("0c2954d2-4c73-44e3-b0f2-c00403e4adef"),
                    Email = "alice@localhost",
                    PasswordHash  = _crypto.HashPassword("alice@localhost", _applicationOptions.PasswordHashingIterationCount),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    EmailVerifiedAt = now,
                    IsLoginAllowed = true,
                    Claims = this.CreateClaims("Alice Smith", "Alice", "Smith")
                },

                // Inactive user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("6b13d17c-55a6-482e-96b9-dc784015f927"),
                    Email = "jim@localhost",
                    PasswordHash  = _crypto.HashPassword("jim@localhost", _applicationOptions.PasswordHashingIterationCount),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = true,
                    EmailVerifiedAt = now,
                    IsLoginAllowed = false,
                    Claims = this.CreateClaims("Jim Panse", "Jim", "Panse"),
                },

                // Not verified user account with local account but no external accounts
                new UserAccount
                {
                    Id = Guid.Parse("13808d08-b1c0-4f28-8d3e-8c9a4051efcb"),
                    Email = "paul@localhost",
                    PasswordHash  = _crypto.HashPassword("paul@localhost", _applicationOptions.PasswordHashingIterationCount),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = false,
                    IsLoginAllowed = false,
                    Claims = this.CreateClaims("Paul Panzer", "Paul", "Panzer")
                    // TODO: set VerificationKey, VerificationPurpose, VerificationKeySentAt
                },

                // External user account
                new UserAccount
                {
                    Id = Guid.Parse("58631b04-9be5-454a-aa1d-f679cd454fa6"),
                    Email = "bob@localhost",
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsEmailVerified = false, // had never confirmed the email, since he got via facebook
                    IsLoginAllowed = true,  // is allowed to login since he registed via facebook
                    Claims = this.CreateClaims("Bob Smith", "Bob", "Smith"),
                    Accounts = new List<ExternalAccount>()
                    {
                        new ExternalAccount
                        {
                            CreatedAt = now,
                            Email = "bob@localhost",
                            Subject = "123456789",
                            Provider = "facebook"
                        }
                    }
                }
            };

            // Map all references
            foreach (var userAccount in userAccounts)
            {
                foreach (var claim in userAccount.Claims)
                {
                    //claim.UserAccountId = userAccount.Id;
                    claim.UserAccount = userAccount;
                }

                if (userAccount.Accounts != null)
                {
                    foreach (var account in userAccount.Accounts)
                    {
                        account.UserAccountId = userAccount.Id;
                        account.UserAccount = userAccount;
                    }
                }

                _defaultDbContext.UserAccounts.Add(userAccount.ToEntity());
            }

            _defaultDbContext.SaveChanges();
        }

        public void CleanupStores()
        {
            throw new NotImplementedException();
        }
    }
}