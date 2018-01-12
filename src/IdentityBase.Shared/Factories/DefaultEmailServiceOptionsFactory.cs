// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Globalization;
    using ServiceBase.Notification.Email;

    public class DefaultEmailServiceOptionsFactory :
        IServiceFactory<DefaultEmailServiceOptions>
    {
        private readonly ThemeHelper _themeHelper;

        public DefaultEmailServiceOptionsFactory(ThemeHelper themeHelper)
        {
            this._themeHelper = themeHelper;
        }

        public DefaultEmailServiceOptions Build()
        {
            return new DefaultEmailServiceOptions
            {
                DefaultLocale = CultureInfo.CurrentUICulture.Name,

                TemplateDirectoryPath =
                    this._themeHelper.GetEmailTemplatesDirectoryPath()
            };
        }
    }
}
