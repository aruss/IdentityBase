using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase.Configuration;
using ServiceBase.IdentityServer.Configuration;
using ServiceBase.IdentityServer.Crypto;
using ServiceBase.IdentityServer.Public.EF;
using ServiceBase.IdentityServer.Extensions;
using ServiceBase.IdentityServer.Services;
using ServiceBase.Notification.Email;
using ServiceBase.Notification.SendGrid;
using ServiceBase.Notification.Sms;
using ServiceBase.Notification.Smtp;
using ServiceBase.Notification.Twilio;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

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
                    confBuilder.AddUserSecrets("ServiceBase.IdentityServer.Public-c23d27a4-eb88-4b18-9b77-2a93f3b15119");
                }
            });

            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<ApplicationOptions>(_configuration.GetSection("App"));

            ConfigureDataLayerServices(services);
            ConfigureIdentityServerServices(services);
            ConfigureEmailSenderServices(services);
            ConfigureSmsSenderServices(services);

            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander());
                });
        }

        internal void ConfigureIdentityServerServices(IServiceCollection services)
        {
            var builder = services.AddIdentityServer((options) =>
            {
                //options.RequireSsl = false;

                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.UserInteraction.LoginUrl = "/login";
                options.UserInteraction.LogoutUrl = "/logout";
                options.UserInteraction.ConsentUrl = "/consent";
                options.UserInteraction.ErrorUrl = "/error";
                options.Authentication.FederatedSignOutPaths.Add("/signout-oidc");
            })
                 .AddProfileService<ProfileService>()
                 .AddSecretParser<ClientAssertionSecretParser>()
                 .AddSecretValidator<PrivateKeyJwtSecretValidator>();

            if (_environment.IsDevelopment())
            {
                builder.AddTemporarySigningCredential();
            }
            else
            {
                // TODO: look for operating system maybe ...

                var cert = new X509Certificate2(Path.Combine(
                    _environment.ContentRootPath, "config/idsvr3test.pfx"), "idsrv3test");

                builder.AddSigningCredential(cert);

                /*builder.AddSigningCredential("98D3ACF057299C3745044BE918986AD7ED0AD4A2",
                    StoreLocation.CurrentUser, nameType: NameType.Thumbprint);*/
            }
        }

        internal void ConfigureDataLayerServices(IServiceCollection services)
        {
            if (String.IsNullOrWhiteSpace(_configuration["EntityFramework"]))
            {
                services.AddEntityFrameworkStores(_configuration.GetSection("EntityFramework"));
            }
            else
            {
                throw new Exception("Store configuration not present");
            }
        }

        internal void ConfigureEmailSenderServices(IServiceCollection services)
        {
            if (String.IsNullOrWhiteSpace(_configuration["Email"]))
            {
                services.Configure<DefaultEmailServiceOptions>(_configuration.GetSection("Email"));

                if (String.IsNullOrWhiteSpace(_configuration["Email:Smtp"]))
                {
                    services.AddTransient<IEmailService, DefaultEmailService>();
                    services.Configure<SmtpOptions>(_configuration.GetSection("Email:Smtp"));
                    services.AddTransient<IEmailSender, SmtpEmailSender>();
                    return;
                }

                if (String.IsNullOrWhiteSpace(_configuration["Email:SendGrid"]))
                {
                    services.AddTransient<IEmailService, DefaultEmailService>();
                    services.Configure<SendGridOptions>(_configuration.GetSection("Email:SendGrid"));
                    services.AddTransient<IEmailSender, SendGridEmailSender>();
                    return;
                }

                // else if o360
                // else if MailGun
            }

            _logger.LogError("Email Service configuration not present");
            services.AddTransient<IEmailService, DebugEmailService>();
        }

        internal void ConfigureSmsSenderServices(IServiceCollection services)
        {
            if (String.IsNullOrWhiteSpace(_configuration["Twillio"]))
            {
                services.AddTransient<ISmsService, DefaultSmsService>();
                services.Configure<TwillioOptions>(_configuration.GetSection("Twillio"));
                services.AddTransient<ISmsSender, TwillioSmsSender>();
            }
            // TODO: Add additional services here
            else
            {
                _logger.LogError("SMS Service configuration not present");
                services.AddTransient<ISmsService, DebugSmsService>();
            }
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

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
            app.UseMiddleware<RequestIdMiddleware>();

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