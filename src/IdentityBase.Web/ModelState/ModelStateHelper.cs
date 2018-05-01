namespace IdentityBase.ModelState
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Newtonsoft.Json;

    public static class ModelStateHelper
    {
        internal static string Key = "TransferedModelState";

        public static string SerialiseModelState(
            ModelStateDictionary modelState)
        {
            IEnumerable<ModelStateTransferValue> errorList = modelState
                .Select(kvp => new ModelStateTransferValue
                {
                    Key = kvp.Key,
                    AttemptedValue = kvp.Value.AttemptedValue,
                    RawValue = kvp.Value.RawValue,
                    ErrorMessages = kvp
                        .Value
                        .Errors
                        .Select(err => err.ErrorMessage)
                        .ToList(),
                });

            return JsonConvert.SerializeObject(errorList);
        }

        public static ModelStateDictionary DeserialiseModelState(
            string serialisedErrorList)
        {
            List<ModelStateTransferValue> errorList =
                JsonConvert.DeserializeObject<List<ModelStateTransferValue>>(
                    serialisedErrorList);

            ModelStateDictionary modelState = new ModelStateDictionary();

            foreach (ModelStateTransferValue item in errorList)
            {
                modelState.SetModelValue(
                    item.Key,
                    item.RawValue,
                    item.AttemptedValue);

                foreach (string error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }

            return modelState;
        }
    }
}
