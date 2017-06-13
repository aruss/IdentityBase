using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelError(this ModelStateDictionary state, string errorMessage)
        {
            state.AddModelError(String.Empty, errorMessage); 
        }
    }
}
