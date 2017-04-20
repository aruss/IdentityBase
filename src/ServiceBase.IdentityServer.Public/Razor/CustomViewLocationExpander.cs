using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;

namespace ServiceBase.IdentityServer.Public.UI
{
    public class CustomViewLocationExpander : IViewLocationExpander
    {
        private readonly string _themePath; 

        // http://benfoster.io/blog/asp-net-core-themes-and-multi-tenancy
        public CustomViewLocationExpander(string themePath)
        {
            _themePath = themePath; 
        }
        
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (!String.IsNullOrWhiteSpace(_themePath))
            {
                yield return String.Format("{0}/Views/{{1}}/{{0}}.cshtml", _themePath);
                yield return String.Format("{0}/Views/Shared/{{0}}.cshtml", _themePath);
            }

            yield return "~/Themes/Default/Views/{1}/{0}.cshtml";
            yield return "~/Themes/Default/Views/Shared/{0}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
