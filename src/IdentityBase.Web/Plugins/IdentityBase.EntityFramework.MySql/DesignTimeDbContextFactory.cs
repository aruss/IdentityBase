// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#if DEBUG

namespace IdentityBase.EntityFramework.MySql
{
    using System.Reflection;
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public static class EntryPoint
    {
        public static void Main()
        {

        }
    }

    /// <summary>
    /// DesignTimeDbContextFactory required for the EF Migration tools 
    /// </summary>
    public class DesignTimeDbContextFactory :
        IDesignTimeDbContextFactory<MigrationDbContext>
    {
        public MigrationDbContext CreateDbContext(string[] args)
        {
            DbContextOptionsBuilder<MigrationDbContext> dbBuilder
                = new DbContextOptionsBuilder<MigrationDbContext>();

            string connString =
                "server=localhost;port=3306;database=identitybase;uid=dev;password=dev";

            string migrationsAssembly = typeof(DesignTimeDbContextFactory)
                .GetTypeInfo().Assembly.GetName().Name;

            dbBuilder.UseMySql(connString,
                o => o.MigrationsAssembly(migrationsAssembly));

            var options = new EntityFrameworkOptions();

            return new MigrationDbContext(dbBuilder.Options, options);
        }
    }
}

#endif
