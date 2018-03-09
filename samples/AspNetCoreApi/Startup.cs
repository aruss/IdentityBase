namespace AspNetCoreApi
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using IdentityServer4.AccessTokenValidation;
    using System;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvcCore()
                .AddJsonFormatters()
                .AddAuthorization();

            services.AddCors();
            services.AddDistributedMemoryCache();

            services
                .AddAuthentication(
                    IdentityServerAuthenticationDefaults.AuthenticationScheme)

                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromSeconds(5); 

                    // used for retrospection calls
                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                });            
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:28895", // AspNetCoreWeb
                    "http://localhost:7017",
                    "http://localhost:3000"); // phonegap app

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}