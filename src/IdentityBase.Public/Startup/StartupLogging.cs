namespace IdentityBase.Public
{
    using Microsoft.AspNetCore.Builder;

    public static class StartupLogging
    {
        public static void UseLogging(this IApplicationBuilder app)
        {
            // Add additional fields to logging context 
            app.Use(async (ctx, next) =>
            {
                var remoteIpAddress = ctx.Request
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
