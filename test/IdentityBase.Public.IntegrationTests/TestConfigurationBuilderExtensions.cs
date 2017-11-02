namespace IdentityBase.Public.IntegrationTests
{
    using System.Collections.Generic;
    using ServiceBase.Tests;
    using System;

    public static class TestConfigurationBuilderExtensions
    {
        public static TestConfigurationBuilder UseDefaultConfiguration(
            this TestConfigurationBuilder builder)
        {
            var configData = new Dictionary<string, string>()
            {
                { "App:PublicUrl", "http://localhost" },
                { "App:TempFolder", $"./AppData/Temp/{Guid.NewGuid()}" },

                { "EntityFramework:MigrateDatabase", "false" },
                { "EntityFramework:SeedExampleData", "true" },
                { "EntityFramework:EnsureDeleted", "true" },
                { "EntityFramework:CleanupTokens", "false" },

                { "Email:TemplateDirectoryPath", "./Templates/Email" },
                { "Sms:TemplateDirectoryPath", "./Templates/Sms" },

                { "Modules:0:Type", "IdentityBase.Public.EntityFramework.InMemoryModule, IdentityBase.Public.EntityFramework" },
                { "Modules:1:Type", "IdentityBase.Public.EntityFramework.ExampleDataStoreInitializerModule, IdentityBase.Public.EntityFramework" },
                { "Modules:2:Type", "IdentityBase.Public.DebugSmsModule, IdentityBase.Public" },
                { "Modules:3:Type", "IdentityBase.Public.DebugEmailModule, IdentityBase.Public" },
                { "Modules:4:Type", "IdentityBase.Public.DefaultEventModule, IdentityBase.Public" },
            };

            return builder.UseConfiguration(configData);
        }

        public static TestConfigurationBuilder RemoveDebugEmailModule(
            this TestConfigurationBuilder builder)
        {
            return builder
                .RemoveByValue("IdentityBase.Public.DebugEmailModule, IdentityBase.Public")
                .ReorderModules(); 
        }
    }
}
