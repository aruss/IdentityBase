using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace IdentityBase.Public
{
    public static class StartupLogging
    {
        public static void UseLogging(
            this IApplicationBuilder app,
            IConfigurationRoot config)
        {
            var loggerFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

            if (Program.Logger != null)
            {
                loggerFactory.AddSerilog(Program.Logger);
            }
            else if (config.ContainsSection("Serilog"))
            {
                loggerFactory.AddSerilog(new LoggerConfiguration()
                   .ReadFrom.ConfigurationSection(config.GetSection("Serilog"))
                   .CreateLogger());
            }

            app.Use(async (ctx, next) =>
            {
                var remoteIpAddress = ctx.Request.HttpContext.Connection.RemoteIpAddress;

                using (Serilog.Context.LogContext
                    .PushProperty("RemoteIpAddress", remoteIpAddress))
                {
                    await next();
                }
            });
        }
    }
}
