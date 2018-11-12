namespace AspNetCoreApi
{
    using System;
    using IdentityServer4.AccessTokenValidation;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ApplicationOptions appOptions = this._configuration
               .GetSection("App")
               .Get<ApplicationOptions>() ?? new ApplicationOptions();

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
                    options.Authority = appOptions.Authority;
                    options.RequireHttpsMetadata = appOptions.Authority
                        .StartsWith("https");

                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromSeconds(5);

                    // Used for retrospection calls
                    options.ApiName = appOptions.ApiName;
                    options.ApiSecret = appOptions.ApiSecret;
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors(policy =>
            {
                policy.WithOrigins(
                    "http://localhost:21402", // AspNetCoreWeb
                    "http://localhost:3000"); // PhoneGap app

                policy.AllowAnyHeader();
                policy.AllowAnyMethod();
                policy.WithExposedHeaders("WWW-Authenticate");
            });

            app.UseAuthentication();
            app.UseMvc();
        }
    }
}