namespace IdentityBase.Public.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Internal;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using ServiceBase;
    using ServiceBase.Logging;

    // Usage 
    // public class Shit
    // {
    //     [Fact]
    //     public void Bar()
    //     {
    //         TestServerBuilder.BuildServerWithDb((server) =>
    //         {
    //             // Do your fancy test here, it will remove the db afterwards
    //             // event if your shit fails while testing 
    //             // throw new ArgumentNullException("FUCK"); 
    //         });
    //     }
    // }
    public class TestServerBuilder2
    {
        private static string GetContentRoot()
        {
            var contentRoot = Path.GetFullPath(Path.Combine(
              Directory.GetCurrentDirectory(), "src/IdentityBase.Public"));

            if (!Directory.Exists(contentRoot))
            {
                contentRoot = Path.GetFullPath(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    "../../../../../src/IdentityBase.Public"));
            }

            return contentRoot;
        }

        public static TestServer BuildServer(
            Dictionary<string, string> config,
            Action<IServiceCollection> configureServicesAction,
            string environmentName = "Test")
        {
            // Create default configuration if none is passed 
            IConfigurationRoot configuration = ConfigBuilder
                .Build(config ?? ConfigBuilder.Default);

            var contentRoot = TestServerBuilder2.GetContentRoot();
            var environment = new HostingEnvironment
            {
                EnvironmentName = environmentName,
                ApplicationName = "IdentityBase.Public",
                ContentRootPath = contentRoot,
                ContentRootFileProvider = new PhysicalFileProvider(contentRoot)
            };

            var logger = new NullLogger<Startup>();
            var startup = new Startup(configuration, environment, logger);

            var builder = new WebHostBuilder()
                .UseContentRoot(contentRoot)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IStartup>(startup);

                    configureServicesAction?.Invoke(services);
                })
                // WORKARROUND: https://github.com/aspnet/Hosting/issues/1137#issuecomment-323234886
                .UseSetting(WebHostDefaults.ApplicationKey,
                    typeof(Startup).GetTypeInfo().Assembly.FullName);

            return new TestServer(builder);
        }

        public static async Task BuildServerAsync(
            Func<TestServer, Task> testAction = null,
            Dictionary<string, string> config = null,
            Action<IServiceCollection> configureServicesAction = null,
            string environmentName = "Test",
            DateTimeAccessor dateTimeAccessor = null)
        {
            // string connString = $"Host=localhost;Database=identitybase_" +
            //      $"{Guid.NewGuid()};Username=postgres;Password=root";
            // 
            Dictionary<string, string> configuration = config ??
                ConfigBuilder.Default;
            // 
            // configuration
            //     .Alter("EntityFramework:Npgsql:ConnectionString", connString);
            // 
            // TestServerBuilder2.CreateSqlDatabase(connString, dateTimeAccessor);

            using (TestServer server = TestServerBuilder2.BuildServer(
                configuration,
                services =>
                {
                    configureServicesAction?.Invoke(services);

                    if (dateTimeAccessor != null)
                    {
                        services
                            .AddSingleton<IDateTimeAccessor>(dateTimeAccessor);
                    }
                },
                environmentName))
            {
                try
                {
                    // Run the shit you call test 
                    await testAction(server);
                }
                catch (Exception)
                {
                    // Let it burn if it crashes 
                    throw;
                }
                finally
                {
                    // But cleanup the shit afterwards
                    // TestServerBuilder2.CleanUpSqlDatabase(connString);
                }
            }
        }

        /// <summary>
        /// Creates database and seeds with test data 
        /// </summary>
        /// <param name="connString"></param>
        // public static void CreateSqlDatabase(
        //     string connString,
        //     DateTimeAccessor dateTimeAccessor = null)
        // {
        //     var dbBuilder = new DbContextOptionsBuilder<MainDbContext>();
        // 
        //     var migrationsAssembly = typeof(DesignTimeDbContextFactory)
        //         .GetTypeInfo().Assembly.GetName().Name;
        // 
        //     dbBuilder.UseNpgsql(connString,
        //         o => o.MigrationsAssembly(migrationsAssembly));
        // 
        //     dateTimeAccessor = dateTimeAccessor ?? new DateTimeAccessor();
        // 
        //     using (var dbContext = new MainDbContext(
        //         dbBuilder.Options,
        //         dateTimeAccessor,
        //         new ProjectX.AppContext
        //         {
        //             // Seed data with jim as babo user 
        //             User = TestDataSeeder.Jim
        //         }))
        //     {
        //         dbContext.Database.Migrate();
        //         dbContext.EnsureBasicData();
        //         dbContext.EnsureTestData(dateTimeAccessor);
        //     }
        // }

        /// <summary>
        /// Removes database 
        /// </summary>
        /// <param name="connString"></param>
        // public static void CleanUpSqlDatabase(string connString)
        // {
        //     var dbBuilder = new DbContextOptionsBuilder<MainDbContext>();
        //     dbBuilder.UseNpgsql(connString);
        // 
        //     using (var dbContext = new MainDbContext(
        //         dbBuilder.Options,
        //         new DateTimeAccessor(),
        //         new ProjectX.AppContext()))
        //     {
        //         dbContext.Database.EnsureDeleted();
        //     }
        // }
    }
}
