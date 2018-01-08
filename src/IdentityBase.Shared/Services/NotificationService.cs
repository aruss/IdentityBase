namespace IdentityBase.Services
{
    using System.Threading.Tasks;
    using IdentityBase.Configuration;
    using IdentityBase.Models;
    using Microsoft.AspNetCore.Http;
    using ServiceBase.Notification.Email;

    /// <summary>
    /// Sends notifications to user in form of push/email/sms
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationOptions _options;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public NotificationService(
            ApplicationOptions options,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor)
        {
            this._options = options;
            this._emailService = emailService;
            this._httpContextAccessor = httpContextAccessor; 
        }

        /// <summary>
        /// Send an email with confirmation and cancelation links in
        /// case of account creation.
        /// </summary>
        /// <param name="userAccount">The instance of created
        /// <see cref="UserAccount"/>.</param>
        public async Task SendUserAccountCreatedEmailAsync(
           UserAccount userAccount)
        {
            string baseUrl = this._httpContextAccessor
                .HttpContext.GetBaseUrl();

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountCreated,
                userAccount.Email,
                new
                {
                    ConfirmUrl =
                        $"{baseUrl}register/confirm/{userAccount.VerificationKey}",

                    CancelUrl =
                        $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
                },
                true
            );
        }
        
        public async Task SendUserAccountRecoverEmailAsync(
            UserAccount userAccount)
        {
            string baseUrl = this._httpContextAccessor
                 .HttpContext.GetBaseUrl();

            await this._emailService.SendEmailAsync(
                EmailTemplates.UserAccountRecover,
                userAccount.Email,
                new
                {
                    ConfirmUrl =
                        $"{baseUrl}recover/confirm/{userAccount.VerificationKey}",

                    CancelUrl =
                        $"{baseUrl}recover/cancel/{userAccount.VerificationKey}"
                },
                true
            );
        }
    }
}
