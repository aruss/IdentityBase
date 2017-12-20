namespace IdentityBase.IntegrationTests
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
                { "App:EnableWebApi", "true" },
                { "App:TempFolder", $"./AppData/Temp/{Guid.NewGuid()}" },

                { "WebApi:AutorityUrl", "http://localhost" },
                { "WebApi:ApiSecret", "secret" },

                { "EntityFramework:MigrateDatabase", "false" },
                { "EntityFramework:SeedExampleData", "true" },
                { "EntityFramework:EnsureDeleted", "true" },
                { "EntityFramework:CleanupTokens", "false" },

                { "Email:TemplateDirectoryPath", "./Templates/Email" },
                { "Sms:TemplateDirectoryPath", "./Templates/Sms" },

                { "Modules:0:Type", "IdentityBase.EntityFramework.InMemoryModule, IdentityBase.EntityFramework" },
                { "Modules:1:Type", "IdentityBase.EntityFramework.ExampleDataStoreInitializerModule, IdentityBase.EntityFramework" },
                { "Modules:2:Type", "IdentityBase.DebugSmsModule, IdentityBase.Shared" },
                { "Modules:3:Type", "IdentityBase.DebugEmailModule, IdentityBase.Shared" },
                { "Modules:4:Type", "IdentityBase.DefaultEventModule, IdentityBase.Shared" },
            };

            return builder.UseConfiguration(configData);
        }

        public static TestConfigurationBuilder RemoveDebugEmailModule(
            this TestConfigurationBuilder builder)
        {
            return builder
                .RemoveByValue("IdentityBase.DebugEmailModule, IdentityBase")
                .ReorderModules();
        }
    }
}
