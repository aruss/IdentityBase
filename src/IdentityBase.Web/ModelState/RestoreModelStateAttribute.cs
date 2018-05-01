namespace IdentityBase.ModelState
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class RestoreModelState : ActionFilterAttribute
    {
        public override void OnActionExecuted(
            ActionExecutedContext filterContext)
        {
            Controller controller = filterContext.Controller as Controller;

            if (controller != null && controller
                    .TempData
                    .ContainsKey(ModelStateHelper.Key))
            {
                string serialisedModelState = controller?
                    .TempData[ModelStateHelper.Key] as string;

                if (serialisedModelState != null)
                {
                    // Only Import if we are viewing
                    if (filterContext.Result is ViewResult)
                    {
                        ModelStateDictionary modelState =
                            ModelStateHelper
                                .DeserialiseModelState(serialisedModelState);

                        filterContext.ModelState.Merge(modelState);
                    }
                    else
                    {
                        // Otherwise remove it.
                        controller.TempData.Remove(ModelStateHelper.Key);
                    }
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
