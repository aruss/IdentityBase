namespace FancyTheme
{
    using System;
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Plugins;
    using ServiceBase.Resources;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder app)
        {
            Console.WriteLine("FancyTheme execute ConfigureAction");

            IResourceStore resourceStore = app.ApplicationServices
                .GetRequiredService<IResourceStore>();

            ApplicationOptions appOptions = app.ApplicationServices
                .GetRequiredService<ApplicationOptions>();

            // TODO: move to install action 
            string localePath = Path.Combine(
                appOptions.PluginsPath,
                "FancyTheme",
                "Resources",
                "Localization");

            resourceStore
                .LoadLocalizationFromDirectoryAsync(
                    localePath,
                    "FancyTheme")
                .Wait();

            string emailTemplatePath = Path.Combine(
                appOptions.PluginsPath,
                "DefaultTheme",
                "Resources",
                "Email");

            resourceStore
                .LoadEmailTemplateFromDirectoryAsync(
                    localePath,
                    "DefaultTheme")
                .Wait();
        }
    }
}
