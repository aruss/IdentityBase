// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class IServiceCollectionExtensions
    {  
        public static IServiceCollection AddFactory<TService, TFactory>(
            this IServiceCollection collection,
            ServiceLifetime serviceLifetime,
            ServiceLifetime factoryLifetime)
            where TService : class
            where TFactory : class, IServiceFactory<TService>
        {
            collection.AddTransient<TFactory>();

            return collection.Add<TService, TFactory>(
                p => p.GetRequiredService<TFactory>(),
                serviceLifetime);
        }

        public static IServiceCollection Add<T, TFactory>(
            this IServiceCollection collection,
            Func<IServiceProvider, TFactory> factoryProvider,
            ServiceLifetime lifetime)
            where T : class
            where TFactory : class, IServiceFactory<T>
        {
            object factoryFunc(IServiceProvider provider)
            {
                TFactory factory = factoryProvider(provider);
                return factory.Build();
            }

            collection.Add(
                new ServiceDescriptor(typeof(T), factoryFunc, lifetime));

            return collection;
        }
    }
}
