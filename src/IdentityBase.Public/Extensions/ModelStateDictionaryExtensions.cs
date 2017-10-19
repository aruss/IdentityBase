namespace Microsoft.AspNetCore.Mvc
{
    using System;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class ModelStateDictionaryExtensions
    {
        public static void AddModelError(
            this ModelStateDictionary state,
            string errorMessage)
        {
            state.AddModelError(String.Empty, errorMessage); 
        }
    }
}
