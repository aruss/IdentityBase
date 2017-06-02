using IdentityBase.Services;
using Microsoft.AspNetCore.Mvc;
using ServiceBase.Authorization;
using System;
using System.Threading.Tasks;

namespace IdentityBase.Public.AdminApi
{
    public class UserAccountsController : ApiController
    {
        private readonly IUserAccountStore _userAccountStore;

        public UserAccountsController(IUserAccountStore userAccountStore)
        {
            _userAccountStore = userAccountStore;
        }

        [HttpGet("useraccounts/{id}")]
        [ScopeAuthorize("useraccount.read")]
        public async Task<object> Get(Guid id)
        {
            return await _userAccountStore.LoadByIdAsync(id);
        }
    }
}
