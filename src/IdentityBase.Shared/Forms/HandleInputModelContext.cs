// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Forms
{
    using Microsoft.AspNetCore.Mvc;

    public class HandleInputModelContext
    {
        public HandleInputModelContext(ControllerBase controller)
        {
            this.Controller = controller;
        }
        
        public ControllerBase Controller { get; private set; }

        /// <summary>
        /// If set to true will stop the execution chain.
        /// </summary>
        public bool Cancel { get; set; }
    }
}
