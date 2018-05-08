namespace DefaultTheme
{
    using System;
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;
    using ServiceBase.Plugins;
    using ServiceBase.Resources;

    public class ConfigureAction : IConfigureAction
    {
        public void Execute(IApplicationBuilder app)
        {
            Console.WriteLine("DefaultTheme execute ConfigureAction");

            IResourceStore resourceStore = app.ApplicationServices
                .GetRequiredService<IResourceStore>();

            ApplicationOptions appOptions = app.ApplicationServices
                .GetRequiredService<ApplicationOptions>();

            IHostingEnvironment env = app.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            // TODO: Move to plugin definition 
            string pluginName = "DefaultTheme";

            string pluginPath = Path.Combine(
                appOptions.PluginsPath.GetFullPath(env.ContentRootPath),
                pluginName); 

            // TODO: Move to install action 
            string localePath = Path.Combine(
                pluginPath,
                "Resources",
                "Localization");

            resourceStore
                .LoadLocalizationFromDirectoryAsync(
                    localePath,
                    pluginName)
                .Wait();

            // TODO: Move to install action
            string emailTemplatePath = Path.Combine(
                pluginPath,
                "Resources",
                "Email");

            resourceStore
                .LoadEmailTemplateFromDirectoryAsync(
                    emailTemplatePath,
                    pluginName)
                .Wait();
        }
    }
}
