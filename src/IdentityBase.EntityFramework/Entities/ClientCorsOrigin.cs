// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#pragma warning disable 1591

namespace IdentityBase.EntityFramework.Entities
{
    using System;

    public class ClientCorsOrigin
    {
        public Guid Id { get; set; }

        public string Origin { get; set; }

        public Client Client { get; set; }
    }
}