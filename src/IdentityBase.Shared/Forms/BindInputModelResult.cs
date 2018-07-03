// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Collections.Generic;

    public class BindInputModelResult
    {
        public BindInputModelResult(IDictionary<string, object> items)
        {
            this.Items = items;
        }

        /// <summary>
        /// Gets a key/value collection that can be used to share 
        /// data within the scope of this procedure.
        /// </summary>
        public IDictionary<string, object> Items { get; private set; }
    }
}
