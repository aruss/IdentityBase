// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace ServiceBase.Modules
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public interface IModule
    {
        void ConfigureServices(
            IServiceCollection services,
            IConfiguration configuration);

        void Configure(IApplicationBuilder app);
    }
}
