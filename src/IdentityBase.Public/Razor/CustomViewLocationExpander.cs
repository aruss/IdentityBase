using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using ServiceBase.Extensions;

namespace IdentityBase.Public.Razor
{
    /// <summary>
    /// Specifies the contracts for a view location expander that is used by 
    /// <see cref="Microsoft.AspNetCore.Mvc.Razor.RazorViewEngine"/> instances 
    /// to determine search paths for a view.
    /// </summary>
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        private readonly string _themePath;

        // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy
        public CustomViewLocationExpander(string themePath = null)
        {
            _themePath = themePath?.EnsureTrailingSlash();
        }

        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            if (!String.IsNullOrWhiteSpace(_themePath))
            {
                yield return String.Format("{0}Views/{{1}}/{{0}}.cshtml", _themePath);
                yield return String.Format("{0}Views/Shared/{{0}}.cshtml", _themePath);
            }

            yield return "~/Themes/Default/Views/{1}/{0}.cshtml";
            yield return "~/Themes/Default/Views/Shared/{0}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
