// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Mvc;

    public class CreateViewModelContext
    {
        public CreateViewModelContext(ControllerBase controller)
        {
            this.Controller = controller;
            this.FormElements = new List<FormElement>();
            this.Items = new Dictionary<string, object>();
        }
        
        public ControllerBase Controller { get; private set; }

        /// <summary>
        /// If set to true will stop the execution chain.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets a key/value collection that can be used to share 
        /// data within the scope of this procedure.
        /// </summary>
        public IDictionary<string, object> Items { get; private set; }

        /// <summary>
        /// Gets a collection of <see cref="FormElement"/>.
        /// </summary>
        public List<FormElement> FormElements { get; private set; }
    }
}
