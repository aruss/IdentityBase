// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase
{
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;

    // TODO: move to ServiceBase 

    /// <summary>
    /// Discovers controllers of specific type from a list of
    /// <see cref="ApplicationPart"/> instances.
    /// </summary>
    public class TypedControllerFeatureProvider<TController> :
        ControllerFeatureProvider where TController : ControllerBase
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!typeof(TController).GetTypeInfo()
                .IsAssignableFrom(typeInfo))
            {
                return false;
            }

            return base.IsController(typeInfo);
        }
    }
}
