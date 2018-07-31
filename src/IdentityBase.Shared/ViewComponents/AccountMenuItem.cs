// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Mvc
{
    public class AccountMenuItem
    {
        public AccountMenuItem(string route, string name = null)
        {
            this.Route = route;
            this.Name = name ?? route;
        }

        public string Route { get; set; }
        public string Name { get; set; }
    }
}
