// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.EntityFramework.Npgsql
{
    public class NpgsqlEfOptions
    {
        public int MaxRetryCount { get; set; } = 3;
        public int MaxRetryDelay { get; set; } = 30;
        public string ConnectionString { get; set; }
    }
}