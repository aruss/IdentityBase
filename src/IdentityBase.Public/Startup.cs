using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using IdentityBase.Configuration;
using IdentityBase.Crypto;
using IdentityBase.Extensions;
using IdentityBase.Services;
using IdentityServer4;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceBase;
using ServiceBase.Configuration;
using System;
using System.IO;

namespace IdentityBase.Public
{
    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup
    {
        private readonly ILogger _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IConfigurationRoot _configuration;
        private IContainer _applicationContainer;

        public Startup(IHostingEnvironment environment, ILoggerFactory loggerFactory)
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

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var options = _configuration.GetSection("App").Get<ApplicationOptions>();
            services.AddSingleton(options);
            services.AddIdentityServer(_configuration, _logger, _environment);
            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<UserAccountService>();
            services.AddTransient<ClientService>();
            services.AddAntiforgery();
            services.AddCors();
            services.AddMvc().AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new Razor.CustomViewLocationExpander(_configuration["App:ThemePath"]));
                });

            if (options.EnableRestApi)
            {
                services.AddRestApi(options); 
            }

            // Create the container builder.
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterInstance(_configuration).As<IConfigurationRoot>().SingleInstance();

            builder.RegisterModule(new ConfigurationModule(_configuration.GetSection("Services")));
            _applicationContainer = builder.Build();
            _applicationContainer.ValidateDataLayerServices(_logger);
            _applicationContainer.ValidateEmailSenderServices(_logger);
            _applicationContainer.ValidateSmsServices(_logger);
            _applicationContainer.ValidateEventServices(_logger);
            return new AutofacServiceProvider(_applicationContainer);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, ApplicationOptions options)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
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
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseStaticFiles(_configuration, _logger, _environment);
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseCors("AllowAll");
            
            if (options.EnableRestApi)
            {
                app.UseRestApi(options); 
            }

            app.InitializeStores();
        }
    }
}