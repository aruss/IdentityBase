// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Theming
{
    using System.Collections.Generic;
    using System.IO;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceBase.Extensions;

    public class ThemedViewLocationExpander : IViewLocationExpander
    {
        private readonly string _themePath;

        public ThemedViewLocationExpander(string themePath)
        {
            if (Path.IsPathRooted(themePath))
            {
                this._themePath = Path
                    .GetFullPath(themePath)
                    .RemoveTrailingSlash();
            }
            else
            {
                this._themePath = themePath
                    .Replace("./", "~/")
                    .RemoveTrailingSlash();
            }
        }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            string themePath = context.Values["ThemePath"];

            yield return $"{themePath}/Views/{{1}}/{{0}}.cshtml";
            yield return $"{themePath}/Views/Shared/{{0}}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            IdentityBaseContext identityBaseContext = context
                .ActionContext
                .HttpContext
                .RequestServices
                .GetService<IdentityBaseContext>();

            // TODO: Move to ClientExtensions
            var clientProperties = identityBaseContext.Client
                .Properties.ToObject<ClientProperties, string>();

            context.Values["ThemePath"] = clientProperties.ThemePath ??
                this._themePath; 
        }
    }
}
