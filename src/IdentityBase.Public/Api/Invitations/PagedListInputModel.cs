namespace IdentityBase.Public.Api.Invitations
{
    public class PagedListInputModel
    {
        public int Take { get; set; } = 50;
        public int Skip { get; set; } = 0;
    }
}