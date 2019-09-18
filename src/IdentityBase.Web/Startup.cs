// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System;
    using System.Net.Http;
    using IdentityBase.Configuration;
    using IdentityBase.Crypto;
    using IdentityBase.Forms;
    using IdentityBase.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ServiceBase;
    using ServiceBase.DependencyInjection;
    using ServiceBase.Extensions;
    using ServiceBase.Localization;
    using ServiceBase.Logging;
    using ServiceBase.Mvc.Theming;
    using ServiceBase.Plugins;

    /// <summary>
    /// Application startup class
    /// </summary>
    public class Startup : IStartup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IHostingEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ApplicationOptions _applicationOptions;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _pluginsPath;

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
            IHttpContextAccessor httpContextAccessor,
            Func<HttpMessageHandler> messageHandlerFactory = null)
        {
            this._logger = loggerFactory.CreateLogger<Startup>();
            this._environment = environment;
            this._configuration = configuration;
            this._httpContextAccessor = httpContextAccessor;

            this._applicationOptions = this._configuration.GetSection("App")
                .Get<ApplicationOptions>() ?? new ApplicationOptions();

            #region Init plugin assembly loader

            // TODO: Add as extension to applicationoptions
            this._pluginsPath = this._applicationOptions.PluginsPath
                .GetFullPath(this._environment.ContentRootPath);

            string[] whiteList = this._configuration
                .GetSection("Plugins")
                .Get<string[]>();

            PluginAssembyLoader.LoadAssemblies(
                this._pluginsPath,
                this._logger,
                whiteList);

            #endregion
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
            this._logger.LogInformation("Configure services");

            services.AddSingleton(this._configuration);
            services.AddSingleton(this._applicationOptions);

            services.AddIdentityServer(
                this._configuration,
                this._logger,
                this._environment);

            services.AddFactory<IdentityBaseContext, IdentityBaseContextIdSrvFactory>();

            services.AddLocalization(
                this._applicationOptions,
                this._environment);

            services.AddTransient<ICryptoService, DefaultCryptoService>();
            services.AddTransient<ClientService>();
            services.AddScoped<UserAccountService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<AuthenticationService>();
            services.AddDefaultForms();
            services.AddHttpClient();

            services.AddSingleton<IEmailProviderInfoService, DefaultEmailProviderInfoService>();

            services.AddAntiforgery((options) =>
            {
                options.Cookie.Name = "idb.srf";
            });

            services.AddSingleton<IDateTimeAccessor, DateTimeAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddDistributedMemoryCache();

            IThemeInfoProvider provider = new ThemeInfoProvider(this._httpContextAccessor);
            services.AddSingleton(provider);
            services.AddPlugins();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization()
                .AddDataAnnotationsLocalization()
                .AddRazorOptions((razor) =>
            {
                razor.ViewLocationExpanders.Clear();
                razor.ViewLocationExpanders.Add(
                    new ThemeViewLocationExpander(provider, this._applicationOptions.PluginsPath));
            });

            services.AddPluginsMvc(this._logger);

            services.AddExternalProviders(this._configuration, this._logger);

            this.OverrideServices?.Invoke(services);

            services.ValidateDataLayerServices(this._logger);
            services.ValidateEmailSenderServices(this._logger);
            services.ValidateEventServices(this._logger);

            this._logger.LogInformation("Services configured");

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
            this._logger.LogInformation("Configure application");

            IHostingEnvironment env = app.ApplicationServices
                .GetRequiredService<IHostingEnvironment>();

            /*app.Use(async (context, next) =>
            {
                context.Request.Scheme = "http";
                context.Request.Host = new HostString("auth.identitybase.local");
                await next.Invoke();
            });*/

            app.UseLocalization();
            app.UseMiddleware<RequestIdMiddleware>();
            app.UseSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseIdentityServer();
            app.UsePluginsStaticFiles(this._pluginsPath);
            app.UsePluginsMvc();
            app.UsePlugins();

            this._logger.LogInformation("Configure application");
        }
    }
}