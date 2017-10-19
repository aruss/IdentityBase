namespace IdentityBase.Extensions
{
    using System.Threading.Tasks;
    using IdentityBase.Models;
    using ServiceBase.Notification.Email;

    public static class IEmailServiceExtensions
    {
        public async static Task SendAccountCreatedEmailAsync(
            this IEmailService emailService,
            UserAccount userAccount)
        {
            await emailService.SendEmailAsync(
                "AccountCreated",
                userAccount.Email,
                new
                {
                    Token = userAccount.VerificationKey
                },
                true);
        }
    }
}
