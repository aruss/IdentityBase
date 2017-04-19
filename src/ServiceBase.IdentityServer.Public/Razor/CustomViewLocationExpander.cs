using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;

namespace ServiceBase.IdentityServer.Public.UI
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        const string DefaultThemeName = "Default";
        private readonly string _themeName; 

        // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy
        public CustomViewLocationExpander(string themeName)
        {
            _themeName = themeName; 
        }
        
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (!_themeName.Equals(DefaultThemeName))
            {
                yield return String.Format("~/Themes/{0}/Views/{{1}}/{{0}}.cshtml", _themeName);
                yield return String.Format("~/Themes/{0}/Views/Shared/{{0}}.cshtml", _themeName);
            }

            yield return "~/Themes/Default/Views/{1}/{0}.cshtml";
            yield return "~/Themes/Default/Views/Shared/{0}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
