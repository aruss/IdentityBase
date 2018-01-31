// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityBase
{
    using System;
    using System.Net.Http;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.Services;
    using IdentityServer4;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase;
    using ServiceBase.Modules;

    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup : IStartup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IHostingEnvironment _environment;
        private readonly ModulesStartup _modulesStartup;
        private readonly IConfiguration _configuration;
        private readonly ApplicationOptions _applicationOptions;
        private readonly Func<HttpMessageHandler> _messageHandlerFactory;

        /// <summary>
        ///
        /// </summary>
        /// <param name="configuration">Instance of <see cref="configuration"/>
        /// </param>
        /// <param name="environment">Instance of
        /// <see cref="IHostingEnvironment"/></param>
        /// <param name="logger">Instance of <see cref="ILogger{Startup}"/>
        /// </param>
        public Startup(
            IConfiguration configuration,
            IHostingEnvironment environment,
            ILoggerFactory loggerFactory,
            Func<HttpMessageHandler> messageHandlerFactory = null)
        {
            this._logger = loggerFactory.CreateLogger<Startup>();
            this._environment = environment;
            this._configuration = configuration;
            this._modulesStartup = new ModulesStartup(configuration);
            this._messageHandlerFactory = messageHandlerFactory;

            this._applicationOptions = this._configuration.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();
        }

        /// <summary>
        /// Configurates the services.
        /// </summary>
        /// <param name="services">
        /// Instance of <see cref="IServiceCollection"/>.
        /// </param>
        /// <returns>
        /// Instance of <see cref="IServiceProvider"/>.
        /// </returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            this._logger.LogInformation("Configure services.");

            services.AddSingleton(this._configuration);
            services.AddSingleton(this._applicationOptions);

            services.AddIdentityServer(
                this._configuration,
                this._logger,
                this._environment);
            
            services.AddFactory<
                IdentityBaseContext,
                IdentityBaseContextIdSrvFactory>(
                    ServiceLifetime.Scoped,
                    ServiceLifetime.Singleton);

            services.AddLocalization(this._applicationOptions, this._environment);
            services.AddTransient<ICrypto, DefaultCrypto>();
            services.AddTransient<ClientService>();
            services.AddScoped<UserAccountService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<AuthenticationService>();
            services.AddScoped<ThemeHelper>();
            services.AddAntiforgery();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IDateTimeAccessor, DateTimeAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // services.AddCors(corsOpts =>
            // {
            //     corsOpts.AddPolicy("CorsPolicy",
            //         corsBuilder => corsBuilder.WithOrigins(
            //             this._configuration.GetValue<string>("Host:Cors")));
            // });

            services.AddDistributedMemoryCache();
            services.AddMvc(this._applicationOptions, this._environment);

            // https://github.com/aspnet/Security/issues/1310
            // services
            //     .AddAuthentication(
            //         IdentityServerConstants.ExternalCookieAuthenticationScheme)
            //     .AddCookie();

            this._modulesStartup.ConfigureServices(services);

            OverrideServices?.Invoke(services);

            services.ValidateDataLayerServices(this._logger);
            services.ValidateEmailSenderServices(this._logger);
            //services.ValidateSmsServices(this._logger);
            services.ValidateEventServices(this._logger);

            this._logger.LogInformation("Services configured.");

            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Only used for testing. Use it for hooking in mocked services.
        /// </summary>
        /// <param name="services"></param>
        public Action<IServiceCollection> OverrideServices { get; set; }

        /// <summary>
        /// Configures the pipeline.
        /// </summary>
        /// <param name="app">
        /// Instance of <see cref="IApplicationBuilder"/>.
        /// </param>
        public virtual void Configure(IApplicationBuilder app)
        {
            this._logger.LogInformation("Configure application.");

            IHostingEnvironment env = app.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            ApplicationOptions options = app.ApplicationServices
                .GetRequiredService<ApplicationOptions>();

            app.UseLocalization();
            // app.UseMiddleware<IdentityBaseContextMiddleware>();
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseLogging();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            //app.UseCors("CorsPolicy");

            app.UseStaticFiles(options, this._environment);
            app.UseIdentityServer();
            //app.UseAuthentication();
            //app.UseWebApi(options);
            app.UseMvcWithDefaultRoute();

            this._modulesStartup.Configure(app);

            this._logger.LogInformation("Configure application.");

            // Run embedded WebAPI if enabled
            if (this._applicationOptions.EnableWebApi)
            {
                app.AddEmbeddedWebApi(this._messageHandlerFactory);
            }
        }
    }
}