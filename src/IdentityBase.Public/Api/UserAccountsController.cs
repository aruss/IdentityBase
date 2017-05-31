using IdentityBase.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using ServiceBase.Authorization;

namespace IdentityBase.Public.AdminApi
{
    public class UserAccountsController : AdminApiController
    {
        private readonly IUserAccountStore _userAccountStore;

        public UserAccountsController(IUserAccountStore userAccountStore)
        {
            _userAccountStore = userAccountStore;
        }

        [HttpGet("useraccounts/{id}")]
        [Authorize]
        //[Authorize("useraccount:read")]

        //[ScopeAuthorize("api2")]
        public async Task<object> Get(Guid id)
        {
            return await _userAccountStore.LoadByIdAsync(id);
        }
    }
}
