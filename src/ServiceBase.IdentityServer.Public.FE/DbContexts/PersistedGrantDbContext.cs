// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using ServiceBase.IdentityServer.Public.EF.Entities;
using ServiceBase.IdentityServer.Public.EF.Extensions;
using ServiceBase.IdentityServer.Public.EF.Interfaces;
using ServiceBase.IdentityServer.Public.EF.Options;
using System;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EF.DbContexts
{
    public class PersistedGrantDbContext : DbContext, IPersistedGrantDbContext
    {
        private readonly EntityFrameworkOptions _storeOptions;

        public PersistedGrantDbContext(DbContextOptions<PersistedGrantDbContext> options, EntityFrameworkOptions storeOptions)
            : base(options)
        {
            if (storeOptions == null) throw new ArgumentNullException(nameof(storeOptions));
            _storeOptions = storeOptions;
        }

        public DbSet<PersistedGrant> PersistedGrants { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.PersistedGrantDbContext(_storeOptions);

            base.OnModelCreating(modelBuilder);
        }
    }
}