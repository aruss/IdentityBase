namespace IdentityBase.Public.Actions.Recover
{
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

    public class RecoverController : Controller
    {
        private readonly ApplicationOptions applicationOptions;
        private readonly ILogger<RecoverController> logger;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IEmailService emailService;
        private readonly ClientService clientService;
        private readonly UserAccountService userAccountService;
        private readonly IHttpContextAccessor httpContextAccessor;

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
            this.applicationOptions = applicationOptions;
            this.logger = logger;
            this.interaction = interaction;
            this.emailService = emailService;
            this.clientService = clientService;
            this.userAccountService = userAccountService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("recover", Name = "Recover")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                logger.LogWarning(IdentityBaseConstants.ErrorMessages
                    .RecoveryNoReturnUrl);

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
                var userAccount = await userAccountService
                    .LoadByEmailAsync(email);

                if (userAccount != null)
                {
                    if (userAccount.IsLoginAllowed)
                    {
                        await userAccountService
                            .SetResetPasswordVirificationKeyAsync(
                                userAccount,
                                model.ReturnUrl);

                        SendEmailAsync(userAccount);

                        return View("Success", new SuccessViewModel()
                        {
                            ReturnUrl = model.ReturnUrl,
                            Provider = userAccount.Email
                                .Split('@')
                                .LastOrDefault()
                        });
                    }
                    else
                    {
                        ModelState.AddModelError(IdentityBaseConstants
                            .ErrorMessages.UserAccountIsDeactivated);
                    }
                }
                else
                {
                    ModelState.AddModelError(IdentityBaseConstants
                        .ErrorMessages.UserAccountDoesNotExists);
                }

                return View(await CreateViewModelAsync(model, userAccount));
            }

            return View(await CreateViewModelAsync(model));
        }

        [HttpGet("recover/confirm/{key}", Name = "RecoverConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await userAccountService.HandleVerificationKeyAsync(
                key,
                VerificationKeyPurpose.ResetPassword
            );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                ModelState.AddModelError(IdentityBaseConstants
                    .ErrorMessages.TokenIsInvalid);
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
            var result = await userAccountService.HandleVerificationKeyAsync(
                model.Key,
                VerificationKeyPurpose.ResetPassword
            );

            if (result.UserAccount == null ||
                result.TokenExpired ||
                !result.PurposeValid)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

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

            await userAccountService.SetNewPasswordAsync(
                result.UserAccount,
                model.Password);

            if (applicationOptions.LoginAfterAccountRecovery)
            {
                await httpContextAccessor.HttpContext
                    .SignInAsync(result.UserAccount, null);

                if (interaction.IsValidReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return this.Redirect(Url.Action("Index", "Login",
                new { ReturnUrl = returnUrl }));
        }

        [HttpGet("recover/cancel/{key}", Name = "RecoverCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await userAccountService.HandleVerificationKeyAsync(
                key,
                VerificationKeyPurpose.ResetPassword
            );

            if (result.UserAccount == null ||
                !result.PurposeValid ||
                result.TokenExpired)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(
                    IdentityBaseConstants.ErrorMessages.TokenIsInvalid);

                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;

            await this.userAccountService
                .ClearVerificationAsync(result.UserAccount);

            return this.Redirect(Url.Action("Index", "Login",
                new { ReturnUrl = returnUrl }));
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(
            string returnUrl)
        {
            return await this.CreateViewModelAsync(
                new RecoverInputModel { ReturnUrl = returnUrl });
        }

        [NonAction]
        internal async Task<RecoverViewModel> CreateViewModelAsync(
            RecoverInputModel inputModel,
            UserAccount userAccount = null)
        {
            var context = await this.interaction
                .GetAuthorizationContextAsync(inputModel.ReturnUrl);

            if (context == null)
            {
                return null;
            }

            var client = await this.clientService
                .FindEnabledClientByIdAsync(context.ClientId);

            var providers = await this.clientService
                .GetEnabledProvidersAsync(client);

            var vm = new RecoverViewModel(inputModel)
            {
                EnableAccountRegistration = applicationOptions
                    .EnableAccountRegistration,

                EnableLocalLogin = (client != null ?
                    client.EnableLocalLogin : false) &&
                    applicationOptions.EnableLocalLogin,

                LoginHint = context.LoginHint,
                ExternalProviders = providers.ToArray(),
                ExternalProviderHints = userAccount?.Accounts?
                    .Select(c => c.Provider)
            };

            return vm;
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            string baseUrl = ServiceBase.Extensions.StringExtensions
                .EnsureTrailingSlash(this.HttpContext
                    .GetIdentityServerBaseUrl());

            await emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountRecover,
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