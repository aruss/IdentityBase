using IdentityBase.Configuration;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Email;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public.Actions.Recover
{
    public class RecoverController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RecoverController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly ClientService _clientService;
        private readonly UserAccountService _userAccountService;

        public RecoverController(
            ApplicationOptions applicationOptions,
            ILogger<RecoverController> logger,
            IUserAccountStore userAccountStore,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            ClientService clientService,
            UserAccountService userAccountService)
        {
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _emailService = emailService;
            _clientService = clientService;
            _userAccountService = userAccountService;
        }

        [HttpGet("recover", Name = "Recover")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = new RecoverViewModel();

            if (!String.IsNullOrWhiteSpace(returnUrl))
            {
                var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
                if (context != null)
                {
                    vm.Email = context.LoginHint;
                    vm.ReturnUrl = returnUrl;

                    if (!String.IsNullOrWhiteSpace(context.ClientId))
                    {
                        var client = await _clientService.FindEnabledClientByIdAsync(context.ClientId);
                        vm.ExternalProviders = await _clientService.GetEnabledProvidersAsync(client);
                        vm.EnableLocalLogin = client != null ? client.EnableLocalLogin : false;
                    }
                }
            }

            return View(vm);
        }

        private async Task<IActionResult> RedirectToSuccessAsync(UserAccount userAccount, string returnUrl)
        {
            // Redirect to success page by preserving the email provider name
            return Redirect(Url.Action("Success", "Recover", new
            {
                returnUrl = returnUrl,
                provider = userAccount.Email.Split('@').LastOrDefault()
            }));
        }

        [HttpPost("recover")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RecoverInputModel model)
        {
            if (ModelState.IsValid)
            {
                // Load user by email
                var email = model.Email.ToLower();

                // Check if user with same email exists
                var userAccount = await _userAccountService.LoadByEmailAsync(email);

                if (userAccount != null)
                {
                    await _userAccountService.SetResetPasswordVirificationKey(userAccount, model.ReturnUrl);

                    var args = new { Key = userAccount.VerificationKey };
                    await _emailService.SendEmailAsync(
                        IdentityBaseConstants.EmailTemplates.UserAccountRecover, userAccount.Email, new
                        {
                            ConfirmUrl = Url.Action("Confirm", "Recover", args, Request.Scheme),
                            CancelUrl = Url.Action("Cancel", "Recover", args, Request.Scheme)
                        }
                    );

                    return await this.RedirectToSuccessAsync(userAccount, model.ReturnUrl);
                }
                else
                {
                    ModelState.AddModelError("", "User is deactivated.");
                }
            }

            var vm = new RecoverViewModel(model);
            return View(vm);
        }

        [HttpGet("recover/success", Name = "RecoverSuccess")]
        public async Task<IActionResult> Success(string returnUrl, string provider)
        {
            // select propper mail provider and render it as button

            return View();
        }

        [HttpGet("recover/confirm/{key}", Name = "RecoverConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key,
                VerificationKeyPurpose.ResetPassword);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("", "Invalid token");
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;

            var vm = new RecoverViewModel
            {
                ReturnUrl = returnUrl,
                Email = result.UserAccount.Email
            };

            return View(vm);
        }

        [HttpGet("recover/cancel/{key}", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key,
                VerificationKeyPurpose.ResetPassword);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("", "Invalid token");
                return View("InvalidToken");
            }

            return Redirect("~/");
        }
    }
}