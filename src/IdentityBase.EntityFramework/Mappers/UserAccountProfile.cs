namespace IdentityBase.EntityFramework.Mappers
{
    using AutoMapper;
    using IdentityBase.Models;
    using ExternalAccountEntity = Entities.ExternalAccount;
    using UserAccountClaimEntity = Entities.UserAccountClaim;
    using UserAccountEntity = Entities.UserAccount;

    public class UserAccountProfile : Profile
    {
        public UserAccountProfile()
        {
            this.CreateMap<UserAccountEntity, UserAccount>(MemberList.Destination).PreserveReferences();

            this.CreateMap<UserAccount, UserAccountEntity>(MemberList.Source).PreserveReferences();

            this.CreateMap<ExternalAccountEntity, ExternalAccount>(MemberList.Destination).PreserveReferences();

            this.CreateMap<ExternalAccount, ExternalAccountEntity>(MemberList.Source).PreserveReferences();

            this.CreateMap<UserAccountClaimEntity, UserAccountClaim>(MemberList.Destination).PreserveReferences();

            this.CreateMap<UserAccountClaim, UserAccountClaimEntity>(MemberList.Source).PreserveReferences();
        }
    }
}