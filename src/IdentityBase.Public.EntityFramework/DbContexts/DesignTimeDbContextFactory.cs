namespace IdentityBase.Public.EntityFramework.DbContexts
{
    using System.IO;
    using System.Reflection;
    using IdentityBase.Public.EntityFramework.Options;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

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
                "Host=localhost;Database=identitybase;Username=postgres;Password=root";

            string migrationsAssembly = typeof(DesignTimeDbContextFactory)
                .GetTypeInfo().Assembly.GetName().Name;

            dbBuilder.UseNpgsql(connString,
                o => o.MigrationsAssembly(migrationsAssembly));

            var options = new EntityFrameworkOptions(); 

            return new MigrationDbContext(dbBuilder.Options, options);
        }
    }
}
