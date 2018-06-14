// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Globalization;
    using ServiceBase.DependencyInjection;
    using ServiceBase.Mvc.Theming;
    using ServiceBase.Notification.Email;

    public class DefaultEmailServiceOptionsFactory :
        IServiceFactory<DefaultEmailServiceOptions>
    {
        private readonly IThemeInfoProvider _themeInfoProvider;

        public DefaultEmailServiceOptionsFactory(
            IThemeInfoProvider themeInfoProvider)
        {
            this._themeInfoProvider = themeInfoProvider;
        }

        public DefaultEmailServiceOptions Build()
        {
            return new DefaultEmailServiceOptions
            {
                DefaultCulture = CultureInfo.CurrentUICulture.Name,

                // TemplateDirectoryPath = ""
                    // this._themeInfoProvider.GetEmailTemplatesDirectoryPath()
            };
        }
    }
}
