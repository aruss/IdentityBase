using IdentityBase.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;

namespace IdentityBase.Public.Api
{
    [Area("Api")]
    public class UserAccountsController : Controller
    {
        private readonly IUserAccountStore _userAccountStore;

        public UserAccountsController(IUserAccountStore userAccountStore)
        {
            _userAccountStore = userAccountStore;
        }
        
        [HttpGet]
        [Route("api/useraccounts/{id}")]
        [Authorize("useraccount:read")]
        public async Task<object> Get(Guid id)
        {
            return await _userAccountStore.LoadByIdAsync(id);
        }
    }
}
