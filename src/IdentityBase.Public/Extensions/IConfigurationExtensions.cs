using System;
using System.Linq;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Contains extensions for <see cref="IConfiguration"/> 
    /// </summary>
    public static class IConfigurationExtensions
    {
        /// <summary>
        /// Determines whether the <see cref="IConfiguration"/> contains the specified section.
        /// </summary>
        /// <param name="config"><see cref="IConfiguration"/></param>
        /// <param name="key">The key to locate in the <see cref="IConfiguration"/></param>
        /// <returns> true if the <see cref="IConfiguration"/> contains a section with the specified key; otherwise, false.</returns>
        public static bool ContainsSection(this IConfiguration config, string key)
        {
            return config.GetChildren().Any(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)); 
        }
    }
}
