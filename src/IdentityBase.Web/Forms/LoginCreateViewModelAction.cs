

namespace IdentityBase.Web.Forms
{
    using System.Threading.Tasks;
    using IdentityBase.Forms;

    public class LoginCreateViewModelAction : ILoginCreateViewModelAction
    {
        public int Step => 0;

        public async Task Execute(CreateViewModelContext context)
        {
            context.FormElements.Add(new FormElement
            {
                Name = "email",
                ViewName = "FormElements/Email"
            });

            context.FormElements.Add(new FormElement
            {
                Name = "password",
                ViewName = "FormElements/Password"
            });

            context.FormElements.Add(new FormElement
            {
                Name = "rememberme",
                ViewName = "FormElements/RememberMe"
            });

            context.FormElements.Add(new FormElement
            {
                Name = "submit",
                ViewName = "FormElements/Submit"
            });
        }
    }



}
