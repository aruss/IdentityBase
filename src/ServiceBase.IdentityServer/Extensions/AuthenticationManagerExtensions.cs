using Microsoft.AspNetCore.Http.Authentication;
using ServiceBase.IdentityServer.Models;
using System.Threading.Tasks;

namespace ServiceBase.IdentityServer.Extensions
{
    public static class AuthenticationManagerExtensions
    {
        public static async Task SignInAsync(
            this AuthenticationManager manager, UserAccount userAccount, AuthenticationProperties properties)
        {
            await manager.SignInAsync(userAccount.Id.ToString(), userAccount.Email, properties);
        }
    }
}