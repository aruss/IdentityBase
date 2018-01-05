namespace IdentityBase
{
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.Extensions.Configuration;
    using ServiceBase.Extensions;

    public static class ThemeHelper
    {
        public static string GetFullThemePath(
            this ApplicationOptions appOptions)
        {
            string path;

            if (Path.IsPathRooted(appOptions.ThemePath))
            {
                path = Path
                    .GetFullPath(appOptions.ThemePath)
                    .RemoveTrailingSlash();
            }
            else
            {
                path = appOptions.ThemePath
                    .RemoveTrailingSlash();
            }

            return path;
        }
        
        public static string GetFullThemePath(
            this IConfiguration configuration)
        {
            return configuration.GetSection("App")
                .Get<ApplicationOptions>().GetFullThemePath(); 
        }
    }
}
