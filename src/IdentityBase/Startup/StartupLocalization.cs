namespace IdentityBase
{
    using System;
    using IdentityBase.Configuration;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly ApplicationOptions _appOptions;
        private readonly ILoggerFactory _loggerFactory;
        private readonly JsonStringLocalizer _stringLocalizer;
        //private readonly ILogger<JsonStringLocalizerFactory> _logger; 

        public JsonStringLocalizerFactory(
            ApplicationOptions appOptions,
            ILoggerFactory loggerFactory)
        {
            this._appOptions = appOptions;
            this._loggerFactory = loggerFactory;

            this._stringLocalizer = new JsonStringLocalizer(
                appOptions,
                loggerFactory.CreateLogger<JsonStringLocalizer>());
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return this._stringLocalizer;
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return this._stringLocalizer;
        }
    }
}

namespace IdentityBase
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using IdentityBase.Configuration;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly ApplicationOptions _appOptions;
        private readonly ILogger<JsonStringLocalizer> _logger;

        private static ConcurrentDictionary<CultureInfo, Dictionary<string, string>>
            _dictionaries;

        public JsonStringLocalizer(
            ApplicationOptions appOptions,
            ILogger<JsonStringLocalizer> logger)
        {
            JsonStringLocalizer._dictionaries =
                new ConcurrentDictionary<CultureInfo, Dictionary<string, string>>();

            this._logger = logger;
            this._appOptions = appOptions;
        }

        private Dictionary<string, string> GetDictionary(CultureInfo culture)
        {
            return JsonStringLocalizer._dictionaries.GetOrAdd(culture, (c) =>
            {
                string resourcePath = Path.Combine(
                    _appOptions.GetFullThemePath(),
                    "Resources",
                    "Localization",
                    $"Shared.{c.Name}.json");

                this._logger.LogInformation(
                    $"Loading localization dictionary: {resourcePath}");

                using (StreamReader r = new StreamReader(resourcePath))
                {
                    string json = r.ReadToEnd();

                    return JsonConvert
                        .DeserializeObject<Dictionary<string, string>>(json);
                }
            });
        }

        private string GetTranslation(string key)
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;
            Dictionary<string, string> dictionary = GetDictionary(culture);

            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            this._logger.LogWarning($"No localization found for \"{key}\"");
            return key;
        }

        public LocalizedString this[string name]
        {
            get
            {
                return new LocalizedString(name, this.GetTranslation(name));
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                string translation = this.GetTranslation(name);

                return new LocalizedString(name,
                    string.Format(translation, arguments));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(
            bool includeParentCultures)
        {
            throw new System.NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}

namespace ServiceBase.Razor.TagHelpers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Razor.TagHelpers;
    using Microsoft.Extensions.Localization;

    [HtmlTargetElement(Attributes = "localize")]
    public class LocalizeHelper : TagHelper
    {
        private readonly IStringLocalizer _localizer;

        public LocalizeHelper(IStringLocalizer localizer)
        {
            this._localizer = localizer;
        }

        [HtmlAttributeName("localize")]
        public string Localize { get; set; }

        public override async Task ProcessAsync(
            TagHelperContext context,
            TagHelperOutput output)
        {
            string key = this.Localize;
            if (String.IsNullOrWhiteSpace(this.Localize))
            {
                TagHelperContent childContent =
                    await output.GetChildContentAsync();

                key = childContent.GetContent().Trim();
            }

            output.Content.SetContent(this._localizer[key]);
        }
    }
}

namespace IdentityBase
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Primitives;

    public class IdentityBaseRequestCultureProvider : IRequestCultureProvider
    {
        public async Task<ProviderCultureResult> DetermineProviderCultureResult(
            HttpContext httpContext)
        {
            var interactionService = httpContext.RequestServices
                .GetService<IIdentityServerInteractionService>();

            var returnUrl = httpContext.Request.Query["ReturnUrl"];

            if (!String.IsNullOrEmpty(returnUrl))
            {
                AuthorizationRequest authContext = await interactionService
                        .GetAuthorizationContextAsync(returnUrl);

                var value = authContext?.Parameters?
                    .GetValues("culture")?.FirstOrDefault();

                if (value != null)
                {
                    var s = new StringSegment(value);
                    return new ProviderCultureResult(s);
                }
            }

            return null;
        }
    }
}

namespace IdentityBase
{
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;

    public class RequestLocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IIdentityServerInteractionService _interactionService;

        public RequestLocalizationMiddleware(RequestDelegate next,
            IIdentityServerInteractionService interactionService)
        {
            this._next = next;
            this._interactionService = interactionService;
        }

        public async Task Invoke(HttpContext context)
        {
            var returnUrl = context.Request.Query["ReturnUrl"];

            AuthorizationRequest authContext = await this._interactionService
                    .GetAuthorizationContextAsync(returnUrl);

            var values = authContext.Parameters.GetValues("culture");

            await this._next(context);
        }
    }
}

namespace IdentityBase
{
    using System.Collections.Generic;
    using System.Globalization;
    using IdentityBase.Configuration;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Localization;
    using ServiceBase.Extensions;

    public static class StartupLocalization
    {
        public static void AddLocalization(
           this IServiceCollection services,
           ApplicationOptions appOptions,
           IHostingEnvironment environment)
        {
            services.TryAdd<IStringLocalizerFactory,
                JsonStringLocalizerFactory>(ServiceLifetime.Singleton);

            services.TryAdd<IStringLocalizer,
                JsonStringLocalizer>(ServiceLifetime.Singleton);

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture =
                    new RequestCulture(appOptions.DefaultCulture);

                options.SupportedCultures =
                options.SupportedUICultures = new List<CultureInfo>
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("de-DE")
                };

                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders
                    .Add(new IdentityBaseRequestCultureProvider());
            });
        }
    }
}
