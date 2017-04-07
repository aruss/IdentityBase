// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
                
using Microsoft.EntityFrameworkCore;
using ServiceBase.IdentityServer.Public.EntityFramework.Entities;
using ServiceBase.IdentityServer.Public.EntityFramework.Extensions;
using ServiceBase.IdentityServer.Public.EntityFramework.Interfaces;
using ServiceBase.IdentityServer.Public.EntityFramework.Options;
using System;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EntityFramework.DbContexts
{
    public class ConfigurationDbContext : DbContext, IConfigurationDbContext
    {
        private readonly EntityFrameworkOptions storeOptions;

        public ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options, EntityFrameworkOptions storeOptions)
            : base(options)
        {
            if (storeOptions == null) throw new ArgumentNullException(nameof(storeOptions));
            this.storeOptions = storeOptions;
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureClientContext(storeOptions);
            modelBuilder.ConfigureResourcesContext(storeOptions);

            base.OnModelCreating(modelBuilder);
        }
    }
}