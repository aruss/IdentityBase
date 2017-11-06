namespace IdentityBase.Public.Actions.Account
{
    public class ConfirmViewModel
    {
        public string Email { get; set; }
        
        public string Key { get; set; }

        public string ReturnUrl { get; set; }
    }
}