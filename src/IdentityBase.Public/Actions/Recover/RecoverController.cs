using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecoverController(
            ApplicationOptions applicationOptions,
            ILogger<RecoverController> logger,
            IUserAccountStore userAccountStore,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            ClientService clientService,
            UserAccountService userAccountService,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _emailService = emailService;
            _clientService = clientService;
            _userAccountService = userAccountService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("recover", Name = "Recover")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                _logger.LogWarning(IdentityBaseConstants.ErrorMessages.RecoveryNoReturnUrl);
                return Redirect(Url.Action("Index", "Error"));
            }

            return View(vm);
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
                    if (userAccount.IsLoginAllowed)
                    {
                        await _userAccountService.SetResetPasswordVirificationKeyAsync(userAccount, model.ReturnUrl);
                        SendEmailAsync(userAccount);

                        return View("Success", new SuccessViewModel()
                        {
                            ReturnUrl = model.ReturnUrl,
                            Provider = userAccount.Email.Split('@').LastOrDefault()
                        });
                    }
                    else
                    {
                        ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.UserAccountIsDeactivated);
                    }
                }
                else
                {
                    ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.UserAccountDoesNotExists);
                }

                return View(await CreateViewModelAsync(model, userAccount));
            }

            return View(await CreateViewModelAsync(model));
        }

        [HttpGet("recover/confirm/{key}", Name = "RecoverConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await _userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ResetPassword);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            var vm = new ConfirmViewModel
            {
                Key = key,
                Email = result.UserAccount.Email
            };

            return View(vm);
        }

        [HttpPost("recover/confirm/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmInputModel model)
        {
            var result = await _userAccountService.HandleVerificationKeyAsync(model.Key,
               VerificationKeyPurpose.ResetPassword);

            if (result.UserAccount == null || result.TokenExpired || !result.PurposeValid)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            if (!ModelState.IsValid)
            {
                return View(new ConfirmViewModel
                {
                    Key = model.Key,
                    Email = result.UserAccount.Email
                }); 
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.SetNewPasswordAsync(result.UserAccount, model.Password);

            if (_applicationOptions.LoginAfterAccountRecovery)
            {
                await _httpContextAccessor.HttpContext.SignInAsync(result.UserAccount, null);

                if (_interaction.IsValidReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return Redirect(Url.Action("Index", "Login", new { ReturnUrl = returnUrl }));
        }

        [HttpGet("recover/cancel/{key}", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await _userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ResetPassword);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.ClearVerificationAsync(result.UserAccount);

            return Redirect(Url.Action("Index", "Login", new { ReturnUrl = returnUrl }));
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(string returnUrl)
        {
            return await CreateViewModelAsync(new RecoverInputModel { ReturnUrl = returnUrl });
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(
            RecoverInputModel inputModel,
            UserAccount userAccount = null)
        {
            var context = await _interaction.GetAuthorizationContextAsync(inputModel.ReturnUrl);
            if (context == null)
            {
                return null;
            }

            var client = await _clientService.FindEnabledClientByIdAsync(context.ClientId);
            var providers = await _clientService.GetEnabledProvidersAsync(client);

            var vm = new RecoverViewModel(inputModel)
            {
                EnableAccountRegistration = _applicationOptions.EnableAccountRegistration,
                EnableLocalLogin = (client != null ? client.EnableLocalLogin : false) && _applicationOptions.EnableLocalLogin,
                LoginHint = context.LoginHint,
                ExternalProviders = providers.ToArray(),
                ExternalProviderHints = userAccount?.Accounts?.Select(c => c.Provider)
            };

            return vm;
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            var baseUrl = ServiceBase.Extensions.StringExtensions.EnsureTrailingSlash(_httpContextAccessor.HttpContext.GetIdentityServerBaseUrl());
            await _emailService.SendEmailAsync(IdentityBaseConstants.EmailTemplates.UserAccountRecover, userAccount.Email, new
            {
                ConfirmUrl = $"{baseUrl}recover/confirm/{userAccount.VerificationKey}",
                CancelUrl = $"{baseUrl}recover/cancel/{userAccount.VerificationKey}"
            }, true);
        }
    }
}