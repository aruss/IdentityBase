using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ServiceBase.Configuration;
using ServiceBase.IdentityServer.Configuration;
using ServiceBase.IdentityServer.Crypto;
using ServiceBase.IdentityServer.Extensions;
using ServiceBase.IdentityServer.Services;
using System;
using System.IO;

namespace ServiceBase.IdentityServer.Public
{
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IConfigurationRoot _configuration;

        public Startup(
            IHostingEnvironment environment,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<Startup>();
            _configuration = ConfigurationSetup.Configure(environment, (confBuilder) =>
            {
                if (environment.IsDevelopment())
                {
                    confBuilder.AddUserSecrets<Startup>();
                }
            });

            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ApplicationOptions>(_configuration.GetSection("App"));

            services.AddIdentityServer(_configuration, _logger, _environment);
            services.AddDataLayer(_configuration, _logger, _environment);
            services.AddSmsSender(_configuration, _logger, _environment);
            services.AddEmailSender(_configuration, _logger, _environment);
            services.AddEvents(_configuration, _logger, _environment);

            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander(_configuration["App:ThemePath"]));
                });

            services.AddCors();

            // Only use for development until this bug is fixed
            // https://github.com/aspnet/DependencyInjection/pull/470
            // return services.BuildServiceProvider(validateScopes: true);
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            /*Func<string, LogLevel, bool> filter = (scope, level) =>
                scope.StartsWith("IdentityServer") ||
                scope.StartsWith("IdentityModel") ||
                level == LogLevel.Error ||
                level == LogLevel.Critical;*/

            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseIdentityServer();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            #region Use third party authentication

            if (!String.IsNullOrWhiteSpace(_configuration["Authentication:Google:ClientId"]))
            {
                _logger.LogInformation("Registering Google authentication scheme");

                app.UseGoogleAuthentication(new GoogleOptions
                {
                    AuthenticationScheme = "Google",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    ClientId = _configuration["Authentication:Google:ClientId"],
                    ClientSecret = _configuration["Authentication:Google:ClientSecret"]
                });
            }

            if (!String.IsNullOrWhiteSpace(_configuration["Authentication:Facebook:AppId"]))
            {
                _logger.LogInformation("Registering Facebook authentication scheme");

                app.UseFacebookAuthentication(new FacebookOptions()
                {
                    AuthenticationScheme = "Facebook",
                    SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme,
                    AppId = _configuration["Authentication:Facebook:AppId"],
                    AppSecret = _configuration["Authentication:Facebook:AppSecret"]
                });
            }

            #endregion Use third party authentication

            var staticFilesPath = _configuration["App:ThemePath"]; 
            if (!String.IsNullOrWhiteSpace(staticFilesPath))
            {
                staticFilesPath = Path.IsPathRooted(staticFilesPath)
                    ? Path.Combine(staticFilesPath, "Public")
                    : Path.Combine(Directory.GetCurrentDirectory(), staticFilesPath, "Public");

                if (Directory.Exists(staticFilesPath))
                {

                }
            }

            app.UseStaticFiles(_configuration, _logger, _environment); 
            app.UseMvcWithDefaultRoute();
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseCors("AllowAll");

            // TODO: if feature "user account api" is enabled
            /*app.Map("/api", apiApp =>
            {
                apiApp.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = "https://demo.identityserver.io",
                    ApiName = "api"
                });

                apiApp.UseMvc();
            });*/

            app.InitializeStores();
        }
    }
}