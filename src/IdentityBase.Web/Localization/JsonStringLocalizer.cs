// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
        private readonly ThemeHelper _themeHelper; 

        private static ConcurrentDictionary
            <CultureInfo, Dictionary<string, string>> _dictionaries;

        public JsonStringLocalizer(
            ApplicationOptions appOptions,
            ILogger<JsonStringLocalizer> logger,
            ThemeHelper themeHelper)
        {
            JsonStringLocalizer._dictionaries = new ConcurrentDictionary
                <CultureInfo, Dictionary<string, string>>();

            this._logger = logger;
            this._appOptions = appOptions;
            this._themeHelper = themeHelper; 
        }

        private Dictionary<string, string> GetDictionary(CultureInfo culture)
        {
            return JsonStringLocalizer._dictionaries.GetOrAdd(culture, (c) =>
            {
                // TODO: Remove dependency on themehelper, solve it via configuration and factory, like DefaultEmailServiceOptionsFactory
                string resourcePath = Path.GetFullPath(Path.Combine(
                    this._themeHelper.GetLocalizationDirectoryPath(),
                    $"Shared.{c.Name}.json"));

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

            Dictionary<string, string> dictionary =
                this.GetDictionary(culture);

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
