namespace IdentityBase
{
    using System.IO;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Extensions;

    public class ThemeHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationOptions _applicationOptions;

        public ThemeHelper(
            ApplicationOptions applicationOptions,
            IHttpContextAccessor httpContextAccessor)
        {
            this._httpContextAccessor = httpContextAccessor;
            this._applicationOptions = applicationOptions;
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

            string path = Path.GetFullPath(
                Path.Combine(
                    this._applicationOptions.ThemeDirectoryPath,
                    theme));

            return path;
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
