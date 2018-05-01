// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;

    public static class ApplicationPartManagerExtensions
    {
        public static void ReplaceControllerFeatureProvider(
            this IList<IApplicationFeatureProvider> featureProviders,
            ControllerFeatureProvider featureProvider)
        {
            // Remove default ControllerFeatureProvider 
            IApplicationFeatureProvider item = featureProviders
                .FirstOrDefault(c => c.GetType()
                    .Equals(typeof(ControllerFeatureProvider)));

            if (item != null)
            {
                featureProviders.Remove(item);
            }

            featureProviders.Add(featureProvider);
        }
    }
}
