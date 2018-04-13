namespace IdentityBase.ModelState
{
    using Microsoft.AspNetCore.Mvc.Filters;

    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }
}
