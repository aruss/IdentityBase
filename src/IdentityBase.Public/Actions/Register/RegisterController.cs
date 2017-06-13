using IdentityBase.Configuration;
using IdentityBase.Extensions;
using IdentityBase.Models;
using IdentityBase.Services;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceBase.Extensions;
using ServiceBase.Notification.Email;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityBase.Public.Actions.Register
{
    public class RegisterController : Controller
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RegisterController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEmailService _emailService;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RegisterController(
            ApplicationOptions applicationOptions,
            ILogger<RegisterController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            ClientService clientService,
            IHttpContextAccessor httpContextAccessor)
        {
            _applicationOptions = applicationOptions;
            _logger = logger;
            _interaction = interaction;
            _emailService = emailService;
            _userAccountService = userAccountService;
            _clientService = clientService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet(IdentityBaseConstants.Routes.Register, Name = "Register")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = new RegisterViewModel();

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

        private async Task SendEmailAsync(UserAccount userAccount)
        {
            var baseUrl = _httpContextAccessor.HttpContext.GetIdentityServerBaseUrl().EnsureTrailingSlash();
            await _emailService.SendEmailAsync(
                IdentityBaseConstants.EmailTemplates.UserAccountCreated, userAccount.Email, new
                {
                    ConfirmUrl = $"{baseUrl}register/confirm/{userAccount.VerificationKey}",
                    CancelUrl = $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
                }
            );
        }

        private async Task<IActionResult> TryCreateNewUserAccount(
            UserAccount userAccount,
            RegisterInputModel model)
        {
            userAccount = await _userAccountService.CreateNewLocalUserAccountAsync(
                        model.Email, model.Password, model.ReturnUrl);

            // Send confirmation mail
            if (_applicationOptions.RequireLocalAccountVerification)
            {
                await SendEmailAsync(userAccount);
            }

            if (_applicationOptions.LoginAfterAccountCreation)
            {
                await _httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null);

                if (model.ReturnUrl != null && _interaction.IsValidReturnUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            return await this.RedirectToSuccessAsync(userAccount, model.ReturnUrl);
        }

        private async Task<IActionResult> RedirectToSuccessAsync(
            UserAccount userAccount,
            string returnUrl)
        {
            // Redirect to success page by preserving the email provider name
            return Redirect(Url.Action("Success", "Register", new
            {
                returnUrl = returnUrl,
                // TODO: load provider info
                provider = userAccount.Email.Split('@').LastOrDefault()
            }));
        }

        private async Task<IActionResult> TryMergeWithExistingUserAccount(
            UserAccount userAccount,
            RegisterInputModel model)
        {
            // Merge accounts without user consent
            if (_applicationOptions.AutomaticAccountMerge)
            {
                await _userAccountService.AddLocalCredentialsAsync(userAccount, model.Password);

                if (_applicationOptions.LoginAfterAccountCreation)
                {
                    await _httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null);

                    if (model.ReturnUrl != null && _interaction.IsValidReturnUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                }
                else
                {
                    return await this.RedirectToSuccessAsync(userAccount, model.ReturnUrl);
                }
            }
            // Ask user if he wants to merge accounts
            else
            {
                throw new NotImplementedException();
            }

            // Return list of external account providers as hint
            var vm = new RegisterViewModel(model);
            vm.HintExternalAccounts = userAccount.Accounts.Select(s => s.Provider).ToArray();
            return View(vm);
        }

        [HttpPost(IdentityBaseConstants.Routes.Register)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterInputModel model)
        {
            if (ModelState.IsValid)
            {
                var email = model.Email.ToLower();

                // Check if user with same email exists
                var userAccount = await _userAccountService.LoadByEmailWithExternalAsync(email);

                // If user dont exists create a new one
                if (userAccount == null)
                {
                    return await this.TryCreateNewUserAccount(userAccount, model);
                }
                // User is just disabled by whatever reason
                else if (!userAccount.IsLoginAllowed)
                {
                    ModelState.AddModelError("Your user account has be disabled");
                }
                // If user has a password then its a local account
                else if (userAccount.HasPassword())
                {
                    // User has to follow a link in confirmation mail
                    if (_applicationOptions.RequireLocalAccountVerification
                        && !userAccount.IsEmailVerified)
                    {
                        ModelState.AddModelError("Please confirm your email account");

                        // TODO: show link for resent confirmation link
                    }

                    // If user has a password then its a local account
                    ModelState.AddModelError("User already exists");
                }
                // External account with same email
                else
                {
                    return await this.TryMergeWithExistingUserAccount(userAccount, model);
                }
            }

            return View(new RegisterViewModel(model));
        }

        [HttpGet(IdentityBaseConstants.Routes.RegisterSuccess, Name = "RegisterSuccess")]
        public async Task<IActionResult> Success(SuccessInputModel model)
        {
            // TODO: Select propper mail provider and render it as button

            var vm = new SuccessViewModel(model);

            return View(vm);
        }

        [HttpGet("register/confirm/{key}", Name = "RegisterConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key, VerificationKeyPurpose.ConfirmAccount);
            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("Invalid token");
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.SetEmailVerifiedAsync(result.UserAccount);

            if (_applicationOptions.EnableUserInviteEndpoint && result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                if (result.UserAccount.HasPassword())
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    _httpContextAccessor.HttpContext.Response.Cookies.Append("ConfirmUserAccountId", result.UserAccount.Id.ToString(), new CookieOptions
                    {
                        Path = "/",
                        HttpOnly = true
                    }); 
                    return Redirect(Url.Action("Complete", "Register", new { ReturnUrl = returnUrl }));
                }
            }

            if (_applicationOptions.LoginAfterAccountConfirmation)
            {
                await _httpContextAccessor.HttpContext.Authentication.SignInAsync(result.UserAccount, null);

                if (returnUrl != null && _interaction.IsValidReturnUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
            }

            return Redirect(Url.Action("Login", "Login", new { ReturnUrl = returnUrl }));           
        }

        [HttpGet("register/cancel/{key}", Name = "RegisterCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await _userAccountService.HandleVerificationKey(key, VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError("Invalid token");
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await _userAccountService.DeleteByIdAsync(result.UserAccount.Id);
            return Redirect(Url.Action("Login", "Login", new { returnUrl = returnUrl }));
        }

        [HttpGet("register/complete")]
        public async Task<IActionResult> Complete(string returnUrl)
        {
            var userAccount = await GetUserAccountFromCoockyValue(); 
            if (userAccount == null)
            {
                ModelState.AddModelError("Invalid token");
                return View("InvalidToken");
            }

            var vm = new RegisterCompleteViewModel
            {
                ReturnUrl = returnUrl,
                UserAccountId = userAccount.Id,
                Email = userAccount.Email
            }; 

            return View(vm);
        }

        internal async Task<UserAccount> GetUserAccountFromCoockyValue()
        {
            string userIdStr;
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("ConfirmUserAccountId", out userIdStr))
            {
                Guid userId;
                if (Guid.TryParse(userIdStr, out userId))
                {
                    return await _userAccountService.LoadByIdAsync(userId);
                }
            }

            return null; 
        }

        [HttpPost("register/complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(RegisterCompleteInputModel model)
        {
            // update user
            // authenticate user
            // redirect to return url

            var userAccount = await GetUserAccountFromCoockyValue();
            _httpContextAccessor.HttpContext.Response.Cookies.Delete("ConfirmUserAccountId");

            // TODO: cleanup
            userAccount.ClearVerification();
            var now = DateTime.UtcNow;
            userAccount.IsLoginAllowed = true;
            userAccount.IsEmailVerified = true;
            userAccount.EmailVerifiedAt = now;
            userAccount.UpdatedAt = now;
            await _userAccountService.AddLocalCredentialsAsync(userAccount, model.Password);

            await _httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null);

            // && _interaction.IsValidReturnUrl(returnUrl)

            if (model.ReturnUrl != null )
            {
                return Redirect(model.ReturnUrl);
            }
            
            return Redirect("/");
        }
    }

    public class RegisterCompleteInputModel
    {
        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [StringLength(100)]
        public string PasswordConfirm { get; set; }

        [StringLength(2000)]
        public string ReturnUrl { get; set; }
    }

    public class RegisterCompleteViewModel : RegisterCompleteInputModel
    {
        public Guid UserAccountId { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; }
    }
}