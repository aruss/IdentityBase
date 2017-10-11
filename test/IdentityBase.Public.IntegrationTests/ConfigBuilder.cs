namespace IdentityBase.Public.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public static class ConfigBuilder
    {
        public static Dictionary<string, string> Default
        {
            get
            {
                var configData = new Dictionary<string, string>()
                {
                    { "App:PublicUrl", "http://localhost" },

                    { "EntityFramework:MigrateDatabase", "false" },
                    { "EntityFramework:SeedExampleData", "true" },
                    { "EntityFramework:EnsureDeleted", "true" },
                    
                    { "Email:TemplateDirectoryPath", "./Templates/Email" },
                    { "Sms:TemplateDirectoryPath", "./Templates/Sms" },

                    { "Authentication:Google:AppId", "530606470939645" },
                    { "Authentication:Google:AppSecret", "8b6544d055a4ef8078a4d182943b0fa3" },
                    { "Authentication:Google:ClientId", "434483408261-55tc8n0cs4ff1fe21ea8df2o443v2iuc.apps.googleusercontent.com" },
                    { "Authentication:Google:ClientSecret", "3gcoTrEDPPJ0ukn_aYYT6PWo" },

                    { "Services:modules:0:type", "IdentityBase.Public.EntityFramework.InMemoryModule, IdentityBase.Public.EntityFramework" },
                    { "Services:modules:1:type", "IdentityBase.Public.EntityFramework.ExampleDataStoreInitializerModule, IdentityBase.Public.EntityFramework" },
                    { "Services:modules:2:type", "IdentityBase.Public.DebugSmsModule, IdentityBase.Public" },
                    { "Services:modules:3:type", "IdentityBase.Public.DebugEmailModule, IdentityBase.Public" },
                    { "Services:modules:4:type", "IdentityBase.Public.DefaultEventModule, IdentityBase.Public" },
                };

                return configData;
            }
        }

        public static Dictionary<string, string> RemoveAuthFacebook(
            this Dictionary<string, string> config)
        {
            return config
                .RemoveByKey("Authentication:Google:AppId")
                .RemoveByKey("Authentication:Google:AppSecret");
        }

        public static Dictionary<string, string> RemoveAuthGoogle(
            this Dictionary<string, string> config)
        {
            return config
                .RemoveByKey("Authentication:Google:ClientId")
                .RemoveByKey("Authentication:Google:ClientSecret");
        }

        public static Dictionary<string, string> RemoveDefaultMailService(
            this Dictionary<string, string> config)
        {
            return config
                .RemoveByValue("IdentityBase.Public.DebugEmailModule, IdentityBase.Public")
                .ReorderServices();
        }

        private static Dictionary<string, string> ReorderServices(
            this Dictionary<string, string> config)
        {
            var items = config
                .Where(c => c.Key.StartsWith("Services:modules:"))
                .ToArray();

            foreach (var item in items)
            {
                config.Remove(item.Key);
            }

            for (int i = 0; i < items.Length; i++)
            {
                config.Add($"Services:modules:{i}:type", items[i].Value);
            }

            return config;
        }

        /// <summary>
        /// Alters the key
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Alter(
            this Dictionary<string, string> config,
            string key,
            string value)
        {
            if (config.ContainsKey(key))
            {
                config[key] = value;
            }
            else
            {
                config.Add(key, value);
            }

            return config;
        }

        /// <summary>
        /// Removes the key
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Dictionary<string, string> RemoveByKey(
            this Dictionary<string, string> config,
            string key)
        {
            if (config.ContainsKey(key))
            {
                config.Remove(key);
            }

            return config;
        }

        public static Dictionary<string, string> RemoveByValue(
            this Dictionary<string, string> config,
            string value)
        {
            var item = config
                .Where(e => e.Value
                    .Equals(value, StringComparison.OrdinalIgnoreCase))
                .Select(e => (KeyValuePair<string, string>?)e)
                .FirstOrDefault();

            if (item.HasValue)
            {
                config.Remove(item.Value.Key);
            }

            return config;
        }

        /// <summary>
        /// Creates a default IdentityBase IConfigurationRoot object 
        /// </summary>
        /// <param name="config">Configuration data</param>
        /// <returns><see cref="IConfigurationRoot"/></returns>
        public static IConfigurationRoot Build(
            this Dictionary<string, string> config)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();
        }

        /// <summary>
        /// Creates a default IdentityBase IConfigurationRoot object 
        /// </summary>
        /// <returns><see cref="IConfigurationRoot"/></returns>
        public static IConfigurationRoot BuildConfig()
        {
            return Default.Build();
        }
    }
}
