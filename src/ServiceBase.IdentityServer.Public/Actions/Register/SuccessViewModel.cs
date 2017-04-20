namespace ServiceBase.IdentityServer.Public.Actions.Register
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