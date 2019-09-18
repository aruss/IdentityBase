namespace DefaultTheme
{
    using System;
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;
    using ServiceBase.Plugins;
    using ServiceBase.Resources;

    public class ConfigureServicesAction : IConfigureServicesAction
    {
        public void Execute(IServiceCollection services)
        {
            IServiceProvider serviceProvider = services
                 .BuildServiceProvider();

            IResourceStore resourceStore = serviceProvider
                .GetRequiredService<IResourceStore>();

            ApplicationOptions appOptions = serviceProvider
                .GetRequiredService<ApplicationOptions>();

            IHostingEnvironment env = serviceProvider
                .GetRequiredService<IHostingEnvironment>();

            // TODO: Move to plugin definition 
            string pluginName = "DefaultTheme";

            string pluginPath = Path.Combine(
                appOptions.PluginsPath.GetFullPath(env.ContentRootPath),
                pluginName);

            // TODO: Move to install action 
            string localePath = Path.Combine(pluginPath, "Resources", "Localization");

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
