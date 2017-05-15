// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using ServiceBase.IdentityServer.Public.EntityFramework.Entities;
using System;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Public.EntityFramework.Interfaces
{
    public interface IPersistedGrantDbContext : IDisposable
    {
        DbSet<PersistedGrant> PersistedGrants { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync();
    }
}