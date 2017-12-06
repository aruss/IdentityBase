// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

namespace IdentityBase.EntityFramework.Entities
{
    using System;
    using System.Collections.Generic;

    public class ApiResource
    {
        public Guid Id { get; set; }

        public bool Enabled { get; set; } = true;

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Description { get; set; }

        public List<ApiSecret> Secrets { get; set; }

        public List<ApiScope> Scopes { get; set; }

        public List<ApiResourceClaim> UserClaims { get; set; }
    }
}