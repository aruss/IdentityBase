using IdentityBase.Models;
using ServiceBase.Notification.Email;
using System.Threading.Tasks;

namespace IdentityBase.Extensions
{
    public static class IEmailServiceExtensions
    {
        public async static Task SendAccountCreatedEmailAsync(this IEmailService emailService, UserAccount userAccount)
        {
            await emailService.SendEmailAsync("AccountCreated", userAccount.Email, new
            {
                Token = userAccount.VerificationKey
            });
        }
    }
}
