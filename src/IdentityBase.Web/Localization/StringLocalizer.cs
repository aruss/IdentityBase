// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityBase
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class StringLocalizerFactory : IStringLocalizerFactory
    {
        private readonly IStringLocalizer _stringLocalizer;

        public StringLocalizerFactory(IStringLocalizer stringLocalizer)
        {
            this._stringLocalizer = stringLocalizer;
            
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

    public class StringLocalizer : IStringLocalizer
    {
        private readonly ILocalizationStore _localizerRepository;
        private readonly ILogger<StringLocalizer> _logger;

        public StringLocalizer(
            ILocalizationStore localizerRepository,
            ILogger<StringLocalizer> logger)
        {
            this._localizerRepository = localizerRepository;
            this._logger = logger;
        }

        private string GetTranslation(string key)
        {
            CultureInfo culture = CultureInfo.CurrentUICulture;

            string translation = this._localizerRepository
                .GetAsync(culture.Name, key)
                .Result;

            if (string.IsNullOrWhiteSpace(translation))
            {
                this._logger.LogWarning($"No localization found for \"{key}\"");
                return key;
            }

            return translation;
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
