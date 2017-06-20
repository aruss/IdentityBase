namespace IdentityBase.Public.Actions.Register
{
    public class ConfirmViewModel : ConfirmInputModel
    {
        public bool RequiresPassword { get; set; }
        public string Email { get; set; }

    }
}