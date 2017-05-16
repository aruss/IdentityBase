// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using IdentityBase.Public.EntityFramework.DbContexts;
using IdentityBase.Public.EntityFramework.Mappers;
using IdentityBase.Public.EntityFramework.Options;
using IdentityBase.Public.EntityFramework.Services;
using ServiceBase.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace IdentityBase.Public.EntityFramework.IntegrationTests.Services
{
    public class CorsPolicyServiceTests : IClassFixture<DatabaseProviderFixture<ConfigurationDbContext>>
    {
        private static readonly EntityFrameworkOptions StoreOptions = new EntityFrameworkOptions();

        public static readonly TheoryData<DbContextOptions<ConfigurationDbContext>> TestDatabaseProviders = new TheoryData<DbContextOptions<ConfigurationDbContext>>
        {
            DatabaseProviderBuilder.BuildInMemory<ConfigurationDbContext>(nameof(CorsPolicyServiceTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlite<ConfigurationDbContext>(nameof(CorsPolicyServiceTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlServer<ConfigurationDbContext>(nameof(CorsPolicyServiceTests), StoreOptions)
        };

        public CorsPolicyServiceTests(DatabaseProviderFixture<ConfigurationDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<ConfigurationDbContext>)y)).ToList();
            fixture.StoreOptions = StoreOptions;
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void IsOriginAllowedAsync_WhenOriginIsAllowed_ExpectTrue(DbContextOptions<ConfigurationDbContext> options)
        {
            const string testCorsOrigin = "https://identityserver.io/";

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.Clients.Add(new Client
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Guid.NewGuid().ToString(),
                    AllowedCorsOrigins = new List<string> { "https://www.identityserver.com" }
                }.ToEntity());
                context.Clients.Add(new Client
                {
                    ClientId = "2",
                    ClientName = "2",
                    AllowedCorsOrigins = new List<string> { "https://www.identityserver.com", testCorsOrigin }
                }.ToEntity());
                context.SaveChanges();
            }

            bool result;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var service = new CorsPolicyService(context, NullLogger<CorsPolicyService>.Create());
                result = service.IsOriginAllowedAsync(testCorsOrigin).Result;
            }

            Assert.True(result);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void IsOriginAllowedAsync_WhenOriginIsNotAllowed_ExpectFalse(DbContextOptions<ConfigurationDbContext> options)
        {
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.Clients.Add(new Client
                {
                    ClientId = Guid.NewGuid().ToString(),
                    ClientName = Guid.NewGuid().ToString(),
                    AllowedCorsOrigins = new List<string> { "https://www.identityserver.com" }
                }.ToEntity());
                context.SaveChanges();
            }

            bool result;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var service = new CorsPolicyService(context, NullLogger<CorsPolicyService>.Create());
                result = service.IsOriginAllowedAsync("InvalidOrigin").Result;
            }

            Assert.False(result);
        }
    }
}