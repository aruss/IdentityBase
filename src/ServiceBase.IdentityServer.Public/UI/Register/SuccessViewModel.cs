namespace ServiceBase.IdentityServer.Public.UI.Register
{
    public class SuccessViewModel : SuccessInputModel
    {
        public SuccessViewModel()
        {
        }

        public SuccessViewModel(SuccessInputModel other)
        {
            this.ReturnUrl = other.ReturnUrl;
        }
    }
}