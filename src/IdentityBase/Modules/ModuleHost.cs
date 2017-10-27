namespace IdentityBase
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class ModuleHost
    {
        private readonly IEnumerable<IModule> modules;
        private readonly IConfiguration configuration;

        public ModuleHost(IConfiguration configuration)
        {
            this.configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));

            ModulesOptions options = configuration.Get<ModulesOptions>();

            this.modules = options.Modules.Select(s =>
            {
                Type type = Type.GetType(s.Type);
                IModule module = (IModule)Activator.CreateInstance(type);

                return module;
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            foreach (IModule module in this.modules)
            {
                module.Configure(app);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (IModule module in this.modules)
            {
                module.ConfigureServices(services, this.configuration);
            }
        }
    }
}
