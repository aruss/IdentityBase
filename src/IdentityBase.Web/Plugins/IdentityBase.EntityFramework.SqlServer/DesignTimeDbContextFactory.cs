#if DEBUG

namespace IdentityBase.EntityFramework.SqlServer
{
    using System.Reflection;
    using IdentityBase.EntityFramework.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

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
                "Server=localhost,1401;Database=IdentityBase;User ID=dev;Password=fancyStrong(!)Password";

            string migrationsAssembly = typeof(DesignTimeDbContextFactory)
                .GetTypeInfo().Assembly.GetName().Name;

            dbBuilder.UseSqlServer(connString,
                o => o.MigrationsAssembly(migrationsAssembly));

            EntityFrameworkOptions options = new EntityFrameworkOptions();

            return new MigrationDbContext(dbBuilder.Options, options);
        }
    }
}

#endif
