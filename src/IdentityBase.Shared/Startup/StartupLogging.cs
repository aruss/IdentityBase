// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Net;
    using Microsoft.AspNetCore.Builder;

    public static class StartupLogging
    {
        public static void UseLogging(this IApplicationBuilder app)
        {
            // Add additional fields to logging context 
            app.Use(async (ctx, next) =>
            {
                IPAddress remoteIpAddress = ctx.Request
                    .HttpContext.Connection.RemoteIpAddress;

                using (Serilog.Context.LogContext
                    .PushProperty("RemoteIpAddress", remoteIpAddress))
                {
                    await next();
                }
            });
        }
    }
}
