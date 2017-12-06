namespace IdentityBase.EntityFramework.IntegrationTests.Stores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using IdentityBase.Models;
    using IdentityBase.EntityFramework.DbContexts;
    using IdentityBase.EntityFramework.Mappers;
    using IdentityBase.EntityFramework.Configuration;
    using IdentityBase.EntityFramework.Stores;
    using Microsoft.EntityFrameworkCore;
    using ServiceBase.Logging;
    using Xunit;

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
        public void LoadByEmailWithExternalAsync_WhenUserAccountExists_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "jim2@panse.de",
                Accounts = new List<ExternalAccount>
                {
                    new ExternalAccount
                    {
                        Provider ="provider",
                        Email = "foo@provider.com",
                        Subject = "123456789"
                    },
                    new ExternalAccount
                    {
                        Provider = "provider",
                        Email = "bar@anotherprovider.com",
                        Subject = "asda5sd4a564da6"
                    }
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

                userAccount = store
                    .LoadByEmailWithExternalAsync(testUserAccount.Email)
                    .Result;
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

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void WriteAsync_NewUserAccount_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount1 = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "foo2@localhost",
                Accounts = new List<ExternalAccount>
                {
                    new ExternalAccount
                    {
                        Email = "foo2@provider",
                        Provider = "facebook",
                        Subject = "123456712",
                    },
                    new ExternalAccount
                    {
                        Email = "bar2@provider",
                        Provider = "google",
                        Subject = "789456111",
                    }
                },
                Claims = new List<UserAccountClaim>
                {
                    new UserAccountClaim("name", "foo2"),
                    new UserAccountClaim("email", "foo2@localhost"),
                }
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                var userAccount = store.WriteAsync(testUserAccount1).Result;
                Assert.NotNull(userAccount);
            }
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void WriteAsync_UpdateUserAccount_ExpectUserAccountRetured(
            DbContextOptions<UserAccountDbContext> options)
        {
            var testUserAccount1 = new UserAccount
            {
                Id = Guid.NewGuid(),
                Email = "foox@localhost",
                Accounts = new List<ExternalAccount>
                {
                    new ExternalAccount
                    {
                        Email = "foox@provider",
                        Provider = "facebook",
                        Subject = "123456789",
                    },
                    new ExternalAccount
                    {
                        Email = "bar@provider",
                        Provider = "google",
                        Subject = "789456123",
                    }
                },
                Claims = new List<UserAccountClaim>
                {
                    new UserAccountClaim("name", "foo"),
                    new UserAccountClaim("email", "foo@localhost"),
                    new UserAccountClaim("w00t", "some junk"),
                }
            };

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                context.UserAccounts.Add(testUserAccount1.ToEntity());
                context.SaveChanges();
                testUserAccount1 = context.UserAccounts.AsNoTracking()
                    .FirstOrDefault(c => c.Id == testUserAccount1.Id)
                    .ToModel();
            }

            UserAccount userAccount;
            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var store = new UserAccountStore(
                    context,
                    NullLogger<UserAccountStore>.Create()
                );

                testUserAccount1.VerificationKeySentAt = DateTime.Now;
                testUserAccount1.VerificationPurpose = 1;
                testUserAccount1.VerificationStorage = "hallo welt";

                userAccount = store.WriteAsync(testUserAccount1).Result;

                Assert.NotNull(userAccount);
            }

            using (var context =
                new UserAccountDbContext(options, StoreOptions))
            {
                var updatedAccount = testUserAccount1 = context.UserAccounts
                    .Include(c => c.Accounts)
                    .Include(c => c.Claims)
                   .FirstOrDefault(c => c.Id == testUserAccount1.Id).ToModel();

                Assert.NotNull(updatedAccount);
                Assert.Equal(
                    updatedAccount.VerificationStorage,
                    userAccount.VerificationStorage);

                Assert.Equal(2, updatedAccount.Accounts.Count());
                Assert.Equal(3, updatedAccount.Claims.Count());
            }
        }
    }
}