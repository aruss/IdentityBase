// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Microsoft.AspNetCore.Mvc
{
    using System;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    /// <summary>
    /// <see cref="ModelStateDictionary"/> extension methods.
    /// </summary>
    public static class ModelStateDictionaryExtensions
    {
        /// <summary>
        /// Adds error message with empty key. 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="errorMessage"></param>
        public static void AddModelError(
            this ModelStateDictionary state,
            string errorMessage)
        {
            state.AddModelError(String.Empty, errorMessage); 
        }
    }
}
