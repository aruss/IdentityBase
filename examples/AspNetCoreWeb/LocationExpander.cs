namespace AspNetCoreWeb
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc.Razor;

    public class LocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(
            ViewLocationExpanderContext context,
            IEnumerable<string> viewLocations)
        {
            yield return "~/Actions/{1}/Views/{0}.cshtml";
            yield return "~/Actions/Shared/{0}.cshtml";
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }
    }
}
