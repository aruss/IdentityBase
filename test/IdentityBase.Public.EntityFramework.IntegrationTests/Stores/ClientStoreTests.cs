// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using IdentityBase.Public.EntityFramework.DbContexts;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Public.EntityFramework.Options;
using IdentityBase.Public.EntityFramework.Stores;
using ServiceBase.Xunit;
using System.Linq;
using Xunit;

namespace IdentityBase.Public.EntityFramework.IntegrationTests.Stores
{
    public class ClientStoreTests : IClassFixture<DatabaseProviderFixture<ConfigurationDbContext>>
    {
        private static readonly EntityFrameworkOptions StoreOptions = new EntityFrameworkOptions();

        public static readonly TheoryData<DbContextOptions<ConfigurationDbContext>> TestDatabaseProviders = new TheoryData<DbContextOptions<ConfigurationDbContext>>
        {
            DatabaseProviderBuilder.BuildInMemory<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlite<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlServer<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions)
        };

        public ClientStoreTests(DatabaseProviderFixture<ConfigurationDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<ConfigurationDbContext>)y)).ToList();
            fixture.StoreOptions = StoreOptions;
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void FindClientByIdAsync_WhenClientExists_ExpectClientRetured(DbContextOptions<ConfigurationDbContext> options)
        {
            var testClient = new Client
            {
                ClientId = "test_client",
                ClientName = "Test Client"
            };

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.Clients.Add(testClient.ToEntity());
                context.SaveChanges();
            }

            Client client;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ClientStore(context, NullLogger<ClientStore>.Create());
                client = store.FindClientByIdAsync(testClient.ClientId).Result;
            }

            Assert.NotNull(client);
        }
    }
}