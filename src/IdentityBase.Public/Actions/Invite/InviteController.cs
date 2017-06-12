using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceBase.Notification.Email;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public.Actions.Invite
{
    public class InviteController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<InviteController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;

        public InviteController(
            ApplicationOptions applicationOptions,
            ILogger<InviteController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            ClientService clientService)
        {
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _emailService = emailService;
            _userAccountService = userAccountService;
            _clientService = clientService;
        }

        [HttpGet("invite/confirm/{key}", Name = "InviteConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key,
                VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("", "Invalid token");
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.SetEmailVerifiedAsync(result.UserAccount);

            // If applicatin settings provided login user after confirmation
            if (_applicationOptions.LoginAfterAccountConfirmation)
            {
                await HttpContext.Authentication.SignInAsync(result.UserAccount, null);

                if (returnUrl != null && _interaction.IsValidReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return Redirect(Url.Action("Login", "Login", new { ReturnUrl = returnUrl }));
        }

        [HttpGet("invite/cancel/{key}", Name = "InviteCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key,
               VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("", "Invalid token");
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.DeleteByIdAsync(result.UserAccount.Id);
            return Redirect(Url.Action("Login", "Login", new { returnUrl = returnUrl }));
        }
    }
}