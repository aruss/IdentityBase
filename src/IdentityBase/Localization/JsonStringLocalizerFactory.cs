// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
            ILoggerFactory loggerFactory,
            ThemeHelper themeHelper)
        {
            this._appOptions = appOptions;
            this._loggerFactory = loggerFactory;

            this._stringLocalizer = new JsonStringLocalizer(
                appOptions,
                loggerFactory.CreateLogger<JsonStringLocalizer>(),
                themeHelper);
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