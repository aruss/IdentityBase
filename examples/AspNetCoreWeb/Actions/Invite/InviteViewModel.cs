namespace AspNetCoreWeb.Actions.Invite
{
    public class InviteViewModel : InviteInputModel
    {
        public InviteViewModel(InviteInputModel inputModel)
        {
            this.Email = inputModel.Email;
        }
    }
}
