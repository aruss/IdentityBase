// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Diagnostics;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase;

    public class Program
    {
        public static void Main(string[] args)
        {
            // Configuration.ExampleDataWriter.Write(); 

            Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");

            WebHostWrapper.Start<Startup>(args, (services) =>
            {
                services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });
        }
    }
}