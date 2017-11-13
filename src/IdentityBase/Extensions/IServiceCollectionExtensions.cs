// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityBase
{
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;

    public static class IServiceCollectionExtensions
    {
        public static bool IsAdded<TService>(
            this IServiceCollection services)
        {
            return services.Any(d => d.ServiceType == typeof(TService)); 
        }        
    }
}
