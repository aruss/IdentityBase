namespace IdentityBase
{
    using System.IO;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Extensions;

    public class ThemeHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationOptions _applicationOptions;
        private readonly IHostingEnvironment _environment; 

        public ThemeHelper(
            ApplicationOptions applicationOptions,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment environment)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._applicationOptions = applicationOptions;
            this._environment = environment; 
        }

        // TODO: return theme object instead of string 
        public string GetTheme()
        {
            HttpContext context = this._httpContextAccessor.HttpContext;

            IdentityBaseContext identityBaseContext =
                    context.GetIdentityBaseContext();

            if (!identityBaseContext.IsValid)
            {
                return "Default"; 
            }

            ClientProperties clientProperties = identityBaseContext.Client
                .Properties.ToObject<ClientProperties>();

            string theme = clientProperties.Theme ?? "Default";

            return theme;
        }

        public string GetThemeDirectoryPath()
        {
            string theme = this.GetTheme();

            if (Path.IsPathRooted(this._applicationOptions.ThemeDirectoryPath))
            {
                return Path.GetFullPath(
                    Path.Combine(
                        this._applicationOptions.ThemeDirectoryPath,
                        theme));
            }
            else
            {
                return Path.GetFullPath(
                  Path.Combine(
                      this._environment.ContentRootPath,
                      this._applicationOptions.ThemeDirectoryPath,
                      theme));                
            }
        }

        public string GetLocalizationDirectoryPath()
        {
            return Path.Combine(
                this.GetThemeDirectoryPath(),
                "Resources",
                "Localization");
        }

        public string GetEmailTemplatesDirectoryPath()
        {
            return Path.Combine(
                this.GetThemeDirectoryPath(),
                "Resources",
                "Email");
        }

        // TODO: add sms templates directory path method here 
    }
}
