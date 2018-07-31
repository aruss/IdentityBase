namespace IdentityBase.EntityFramework.IntegrationTests.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.DbContexts;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.EntityFramework.Stores;
    using IdentityBase.Models;
    using Microsoft.EntityFrameworkCore;
    using ServiceBase.Logging;
    using Xunit;

    [Collection("UserAccountStoreTests")]
    public class UserAccountStoreTests :
        IClassFixture<DatabaseProviderFixture<UserAccountDbContext>>
    {
        private static readonly EntityFrameworkOptions StoreOptions =
            new EntityFrameworkOptions();

        public static readonly TheoryData<DbContextOptions<UserAccountDbContext>>
            TestDatabaseProviders = new TheoryData<DbContextOptions<UserAccountDbContext>>
        {
                DatabaseProviderBuilder.BuildInMemory<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions),

                DatabaseProviderBuilder.BuildSqlite<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions),

                DatabaseProviderBuilder.BuildSqlServer<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions)
            };

        public UserAccountStoreTests(
            DatabaseProviderFixture<UserAccountDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders
                .SelectMany(x => x
                    .Select(y => (DbContextOptions<UserAccountDbContext>)y))
                .ToList();

            fixture.StoreOptions = StoreOptions;
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void LoadByIdAsync_WhenUserAccountExists_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "jim@panse.de"
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                context.UserAccounts.Add(testUserAccount.ToEntity());
                context.SaveChanges();
            }

            UserAccount userAccount;
            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context, NullLogger<UserAccountStore>.Create());

                userAccount = store.LoadByIdAsync(testUserAccount.Id).Result;
            }

            Assert.NotNull(userAccount);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void LoadByEmailAsync_WhenUserAccountExists_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount = new UserAccount
            {
                Email = "jim22@panse.de"
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                context.UserAccounts.Add(testUserAccount.ToEntity());
                context.SaveChanges();
            }

            UserAccount userAccount;
            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                userAccount = store
                    .LoadByEmailAsync(testUserAccount.Email).Result;
            }

            Assert.NotNull(userAccount);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void LoadByVerificationKeyAsync_WhenUserAccountExists_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "jim3@panse.de",
                VerificationKey = Guid.NewGuid().ToString()
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                context.UserAccounts.Add(testUserAccount.ToEntity());
                context.SaveChanges();
            }

            UserAccount userAccount;
            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                userAccount = store.LoadByVerificationKeyAsync(
                    testUserAccount.VerificationKey).Result;
            }

            Assert.NotNull(userAccount);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void LoadByExternalProviderAsync_WhenUserAccountExists_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testExternalAccount = new ExternalAccount
            {
                Email = "jim4@panse.de",
                Provider = "yahoo",
                Subject = "123456789"
            };

            var testUserAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "jim4@panse.de",
                Accounts = new List<ExternalAccount>
                {
                    testExternalAccount
                }
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                context.UserAccounts.Add(testUserAccount.ToEntity());
                context.SaveChanges();
            }

            UserAccount userAccount;
            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                userAccount = store.LoadByExternalProviderAsync(
                    testExternalAccount.Provider,
                    testExternalAccount.Subject).Result;
            }

            Assert.NotNull(userAccount);
        }

        // Create user account
        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void Create_UserAccount(
            DbContextOptions<UserAccountDbContext> options)
        {
            var userAccount1 = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "foo@bar.com",
                Accounts = new List<ExternalAccount>
                {
                    new ExternalAccount
                    {
                        Email = "foo@bar.com",
                        Provider = "facebook",
                        Subject = "123456712",
                    },
                    new ExternalAccount
                    {
                        Email = "foo@bar.com",
                        Provider = "google",
                        Subject = "789456111",
                    }
                },
                Claims = new List<UserAccountClaim>
                {
                    new UserAccountClaim("name", "foo"),
                    new UserAccountClaim("email", "foo@bar.com"),
                }
            };

            using (var context =
              new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                // Write user account 
                UserAccount userAccount2 = store.WriteAsync(userAccount1).Result;

                // Assert user account 
                Assert.NotNull(userAccount2);
                // TODO: check other fields

                // Assert claims
                Assert.Equal(2, userAccount2.Claims.Count());

                Assert.NotNull(userAccount2.Claims.FirstOrDefault(c =>
                    c.Type.Equals("name") &&
                    c.Value.Equals("foo")));

                Assert.NotNull(userAccount2.Claims.FirstOrDefault(c =>
                    c.Type.Equals("email") &&
                    c.Value.Equals("foo@bar.com")));

                // Assert external accounts
                Assert.Equal(2, userAccount2.Accounts.Count());

                Assert.NotNull(userAccount2.Accounts.FirstOrDefault(c =>
                  c.Email.Equals("foo@bar.com") &&
                  c.Provider.Equals("facebook") &&
                  c.Subject.Equals("123456712")));

                Assert.NotNull(userAccount2.Accounts.FirstOrDefault(c =>
                    c.Email.Equals("foo@bar.com") &&
                    c.Provider.Equals("google") &&
                    c.Subject.Equals("789456111")));
            }
        }
        
        // Delete user account 
        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void Delete_UserAccount(
           DbContextOptions<UserAccountDbContext> options)
        {
            var userAccountId = Guid.NewGuid();

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var userAccount1 = new UserAccount
                {
                    Id = userAccountId,
                    Email = "foo@bar.com",
                    Accounts = new List<ExternalAccount>
                    {
                        new ExternalAccount
                        {
                            Email = "foo@bar.com",
                            Provider = "facebook",
                            Subject = "123456712",
                        },
                        new ExternalAccount
                        {
                            Email = "foo@bar.com",
                            Provider = "google",
                            Subject = "789456111",
                        }
                    },
                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim("name", "foo"),
                        new UserAccountClaim("email", "foo@bar.com"),
                    }
                };

                context.UserAccounts.Add(userAccount1.ToEntity());
                context.SaveChanges();
            }

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                store.DeleteByIdAsync(userAccountId).Wait();
            }

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                Assert.Null(context.UserAccounts
                    .FirstOrDefault(c => c.Id.Equals(userAccountId)));

                Assert.Null(context.ExternalAccounts
                    .FirstOrDefault(c => c.UserAccountId.Equals(userAccountId)));

                Assert.Null(context.UserAccountClaims
                    .FirstOrDefault(c => c.UserAccountId.Equals(userAccountId)));
            }
        }
    }

    [Collection("UserAccountStoreTests")]
    public class UserAccountStore2Tests :
         IClassFixture<DatabaseProviderFixture<UserAccountDbContext>>
    {
        private static readonly EntityFrameworkOptions StoreOptions =
            new EntityFrameworkOptions();

        public static readonly TheoryData<DbContextOptions<UserAccountDbContext>>
            TestDatabaseProviders = new TheoryData<DbContextOptions<UserAccountDbContext>>
        {
                DatabaseProviderBuilder.BuildInMemory<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions),

                DatabaseProviderBuilder.BuildSqlite<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions),

                DatabaseProviderBuilder.BuildSqlServer<UserAccountDbContext>(
                    nameof(UserAccountStoreTests), StoreOptions)
            };

        public UserAccountStore2Tests(
            DatabaseProviderFixture<UserAccountDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders
                .SelectMany(x => x
                    .Select(y => (DbContextOptions<UserAccountDbContext>)y))
                .ToList();

            fixture.StoreOptions = StoreOptions;
        }
        // Update user account
        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void Update_UserAccount(
            DbContextOptions<UserAccountDbContext> options)
        {
            var userAccountId = Guid.NewGuid();
            var now = DateTime.Now;

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var userAccount1 = new UserAccount
                {
                    Id = userAccountId,
                    Email = "foo@bar.com",
                    Accounts = new List<ExternalAccount>
                    {
                        new ExternalAccount
                        {
                            Email = "foo@bar.com",
                            Provider = "facebook",
                            Subject = "123456712",
                        },
                        new ExternalAccount
                        {
                            Email = "foo@bar.com",
                            Provider = "google",
                            Subject = "789456111",
                        }
                    },
                    Claims = new List<UserAccountClaim>
                    {
                        new UserAccountClaim("name", "foo"),
                        new UserAccountClaim("email", "foo@bar.com"),
                    }
                };

                context.UserAccounts.Add(userAccount1.ToEntity());
                context.SaveChanges();
            }

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                var userAccount2 = store.LoadByIdAsync(userAccountId).Result;
                userAccount2.VerificationKeySentAt = now;
                userAccount2.VerificationPurpose = 1;
                userAccount2.VerificationStorage = "hallo welt";

                var claims = userAccount2.Claims as List<UserAccountClaim>;

                // Update one claim
                claims.FirstOrDefault(c =>
                   c.Type.Equals("name") &&
                   c.Value.Equals("foo"))
                   .Value = "bar";

                // Remove another claim
                claims.Remove(claims
                    .FirstOrDefault(c => c.Type.Equals("email")));

                // Add one extra claim
                //  claims.Add(new UserAccountClaim("bar", "baz"));

                var accounts = userAccount2.Accounts as List<ExternalAccount>;

                // Update one external account
                accounts.FirstOrDefault(c =>
                 c.Email.Equals("foo@bar.com") &&
                 c.Provider.Equals("facebook") &&
                 c.Subject.Equals("123456712"))
                 .Email = "baz@foo.com";

                // remove another external account
                accounts.Remove(accounts.FirstOrDefault(c =>
                   c.Email.Equals("foo@bar.com") &&
                   c.Provider.Equals("google") &&
                   c.Subject.Equals("789456111")));

                //// Add one extra external account
                //accounts.Add(new ExternalAccount
                //{
                //    Email = "foo@bar.com",
                //    Provider = "yahoo",
                //    Subject = "654987132",
                //});

                var userAccount3 = store.WriteAsync(userAccount2).Result;
            }

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                var userAccount4 = store.LoadByIdAsync(userAccountId).Result;

                Assert.NotNull(userAccount4);
                Assert.Equal(now, userAccount4.VerificationKeySentAt);
                Assert.Equal(1, userAccount4.VerificationPurpose);
                Assert.Equal("hallo welt", userAccount4.VerificationStorage);
                /*
               // Assert claims
               Assert.Equal(2, userAccount4.Claims.Count());

               Assert.NotNull(userAccount4.Claims.FirstOrDefault(c =>
                   c.Type.Equals("name") &&
                   c.Value.Equals("bar")));

               Assert.NotNull(userAccount4.Claims.FirstOrDefault(c =>
                   c.Type.Equals("bar") &&
                   c.Value.Equals("baz")));

               // Assert external accounts
               Assert.Equal(2, userAccount4.Accounts.Count());

               Assert.NotNull(userAccount4.Accounts.FirstOrDefault(c =>
                 c.Email.Equals("baz@foo.com") &&
                 c.Provider.Equals("facebook") &&
                 c.Subject.Equals("123456712")));

               Assert.NotNull(userAccount4.Accounts.FirstOrDefault(c =>
                   c.Email.Equals("foo@bar.com") &&
                   c.Provider.Equals("yahoo") &&
                   c.Subject.Equals("654987132")));*/

            }
        }
    }
}