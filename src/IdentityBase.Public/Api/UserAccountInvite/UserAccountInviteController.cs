using Microsoft.AspNetCore.Mvc;
using ServiceBase.Authorization;

namespace IdentityBase.Public.Api.UserAccountInvite
{
    public class UserAccountInviteController : ApiController
    {
        public UserAccountInviteController()
        {

        }

        [HttpPut("users/_invite")]
        [ScopeAuthorize("user.invite")]
        public UserAccountInviteResponse Put(UserAccountInviteRequest inputModel)
        {
            
            return new UserAccountInviteResponse
            {
                
            };
        }
    }

    public class UserAccountInviteRequest
    {
        public string Email { get; set; }
    }

    public class UserAccountInviteResponse
    {

    }
}
