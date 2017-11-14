// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace ServiceBase.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class ModuleHost
    {
        private readonly IEnumerable<IModule> _modules;
        private readonly IConfiguration _configuration;

        public ModuleHost(IConfiguration configuration)
        {
            this._configuration = configuration ??
                throw new ArgumentNullException(nameof(configuration));

            ModulesOptions options = configuration.Get<ModulesOptions>();

            this._modules = options.Modules
                .Select(s =>
                {
                    Type type = Type.GetType(s.Type);

                    if (type == null)
                    {
                        throw new ApplicationException(
                            $"Cannot load type \"{s.Type}\"");
                    }

                    IModule module = (IModule)Activator.CreateInstance(type);
                    return module;
                }
            );
        }

        public void Configure(IApplicationBuilder app)
        {
            foreach (IModule module in this._modules)
            {
                module.Configure(app);
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            foreach (IModule module in this._modules)
            {
                module.ConfigureServices(services, this._configuration);
            }
        }
    }
}
