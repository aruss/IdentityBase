// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    
    public class BlackListedControllerFeatureProvider :
        ControllerFeatureProvider
    {
        IEnumerable<TypeInfo> blackList;

        public BlackListedControllerFeatureProvider(
            IEnumerable<TypeInfo> blackList)
        {
            this.blackList = blackList;
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo) &&
                !this.blackList.Contains(typeInfo);
        }
    }
}