// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Theming
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            string theme = context.Values["theme"];

            yield return $"~/Themes/{theme}/Views/{{1}}/{{0}}.cshtml";
            yield return $"~/Themes/{theme}/Views/Shared/{{0}}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            ThemeHelper themeHelper = context
                .ActionContext
                .HttpContext
                .RequestServices.GetService<ThemeHelper>();

            context.Values["theme"] = themeHelper.GetTheme();
        }
    }
}
