// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Diagnostics;

    public class Program
    {
        public static void Main(string[] args)
        {
            int processId = Process.GetCurrentProcess().Id;
            Console.WriteLine($"Process ID: {processId}");
            Console.Title = $"IdentityBase.Web ({processId})"; 

            IdentityBaseWebHost.Run<Startup>(args);
        }
    }
}