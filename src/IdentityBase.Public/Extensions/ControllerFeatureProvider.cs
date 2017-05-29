using Microsoft.AspNetCore.Mvc.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Mvc.ApplicationParts
{
    /// <summary>
    /// Discovers controllers from a list of <see cref="ApplicationPart"/> instances.
    /// </summary>
    public class ControllerFeatureProvider<TControllerBase> : IApplicationFeatureProvider<ControllerFeature> where TControllerBase : ControllerBase
    {
        private Type _controllerType;

        /// <inheritdoc />
        public ControllerFeatureProvider()
        {
            _controllerType = typeof(TControllerBase);
        }

        /// <summary>
        /// Updates the feature intance.
        /// </summary>
        /// <param name="parts">The list of Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationParts of the application.</param>
        /// <param name="feature">The feature instance to populate.</param>
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var part in parts.OfType<IApplicationPartTypeProvider>())
            {
                foreach (var type in part.Types)
                {
                    if (type.IsSubclassOf(_controllerType) && !feature.Controllers.Contains(type))
                    {
                        feature.Controllers.Add(type);
                    }
                }
            }
        }
    }
}
