namespace ServiceBase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using MsConfig = Microsoft.Extensions.Configuration;

    public class TestConfigurationBuilder
    {
        private Dictionary<string, string> config;

        public TestConfigurationBuilder UseConfiguration(
            Dictionary<string, string> config)
        {

            this.config = config;
            return this;
        }
        
        /// <summary>
        /// Alters the key
        /// </summary>
        /// <param name="config"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public TestConfigurationBuilder Alter(string key, string value)
        {
            if (this.config.ContainsKey(key))
            {
                this.config[key] = value;
            }
            else
            {
                this.config.Add(key, value);
            }

            return this;
        }

        /// <summary>
        /// Removes by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TestConfigurationBuilder RemoveByKey(string key)
        {
            if (this.config.ContainsKey(key))
            {
                this.config.Remove(key);
            }

            return this;
        }

        /// <summary>
        /// Removes by value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public TestConfigurationBuilder RemoveByValue(string value)
        {
            var item = this.config
                .Where(e => e.Value
                    .Equals(value, StringComparison.OrdinalIgnoreCase))
                .Select(e => (KeyValuePair<string, string>?)e)
                .FirstOrDefault();

            if (item.HasValue)
            {
                this.config.Remove(item.Value.Key);
            }

            return this;
        }

        public TestConfigurationBuilder ReorderModules()
        {
            var items = this.config
                .Where(c => c.Key.StartsWith("Modules"))
                .ToArray();

            foreach (var item in items)
            {
                config.Remove(item.Key);
            }

            for (int i = 0; i < items.Length; i++)
            {
                this.config.Add($"Modules:{i}:Type", items[i].Value);
            }

            return this;
        }

        /// <summary>
        /// Creates a default IdentityBase IConfigurationRoot object 
        /// </summary>
        /// <returns><see cref="IConfiguration"/></returns>
        public IConfiguration Build()
        {
            return new MsConfig.ConfigurationBuilder()
                .AddInMemoryCollection(this.config)
                .Build();
        }        
    }
}
