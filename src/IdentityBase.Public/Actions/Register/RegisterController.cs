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

        [HttpGet("register", Name = "Register")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                _logger.LogError("Register attempt with missing returnUrl parameter");
                return Redirect(Url.Action("Index", "Error"));
            }

            return View(vm);
        }

        [HttpPost("register")]
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
                    if (_applicationOptions.RequireLocalAccountVerification && !userAccount.IsEmailVerified)
                    {
                        ModelState.AddModelError("Please confirm your email account");

                        // TODO: show link for resent confirmation link
                    }

                    // If user has a password then its a local account
                    ModelState.AddModelError("User already exists");
                }
                else
                {
                    // External account with same email
                    return await TryMergeWithExistingUserAccount(userAccount, model);
                }

                return View(await CreateViewModelAsync(model, userAccount));
            }

            return View(await CreateViewModelAsync(model));
        }

        [HttpGet("register/confirm/{key}", Name = "RegisterConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await _userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            // User account requires completion 
            if (_applicationOptions.EnableUserInviteEndpoint &&
                result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                var vm = new ConfirmViewModel
                {
                    Key = key,
                    RequiresPassword = !result.UserAccount.HasPassword()
                };

                return View(vm);
            }
            // User profile already fine and just needs to be activated
            else
            {
                var returnUrl = result.UserAccount.VerificationKey;
                await _userAccountService.SetEmailVerifiedAsync(result.UserAccount);

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
        }

        // Currently is only used for invitations 
        [HttpPost("register/confirm/{key}")]
        public async Task<IActionResult> Confirm(ConfirmInputModel model)
        {
            if (!_applicationOptions.EnableUserInviteEndpoint)
            {
                return NotFound(); 
            }

            var result = await _userAccountService.HandleVerificationKeyAsync(model.Key,
               VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || result.TokenExpired || !result.PurposeValid)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            _userAccountService.SetEmailVerified(result.UserAccount);
            _userAccountService.AddLocalCredentials(result.UserAccount, model.Password);
            await _userAccountService.UpdateUserAccountAsync(result.UserAccount);
            
            if (result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                // TODO: validate 
                return Redirect(returnUrl);
            }
            else
            {
                if (_applicationOptions.LoginAfterAccountRecovery)
                {
                    await _httpContextAccessor.HttpContext.Authentication.SignInAsync(result.UserAccount, null);

                    if (_interaction.IsValidReturnUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                }

                return Redirect(Url.Action("Index", "Login", new { ReturnUrl = returnUrl }));
            }       
        }

        [HttpGet("register/cancel/{key}", Name = "RegisterCancel")]
        public async Task<IActionResult> Cancel(string key)
        {
            var result = await _userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ConfirmAccount);

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

        //   [HttpGet("register/complete")]
        //   public async Task<IActionResult> Complete(string returnUrl)
        //   {
        //       var userAccount = await GetUserAccountFromCoockyValue();
        //       if (userAccount == null)
        //       {
        //           ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
        //           return View("InvalidToken");
        //       }
        //
        //       var vm = new RegisterCompleteViewModel
        //       {
        //           ReturnUrl = returnUrl,
        //           UserAccountId = userAccount.Id,
        //           Email = userAccount.Email
        //       };
        //
        //       return View(vm);
        //   }

        //   [HttpPost("register/complete")]
        //   [ValidateAntiForgeryToken]
        //   public async Task<IActionResult> Complete(RegisterCompleteInputModel model)
        //   {
        //       // update user
        //       // authenticate user
        //       // redirect to return url
        //
        //       var userAccount = await GetUserAccountFromCoockyValue();
        //       _httpContextAccessor.HttpContext.Response.Cookies.Delete("ConfirmUserAccountId");
        //
        //       throw new NotImplementedException();
        //
        //       /*
        //       // TODO: cleanup
        //       userAccount.ClearVerification();
        //       var now = DateTime.UtcNow;
        //       userAccount.IsLoginAllowed = true;
        //       userAccount.IsEmailVerified = true;
        //       userAccount.EmailVerifiedAt = now;
        //       userAccount.UpdatedAt = now;
        //
        //       await Task.WhenAll(
        //           _userAccountService.AddLocalCredentialsAsync(userAccount, model.Password),
        //           _httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null)
        //       ); */
        //
        //       // && _interaction.IsValidReturnUrl(returnUrl)
        //
        //       if (model.ReturnUrl != null)
        //       {
        //           return Redirect(model.ReturnUrl);
        //       }
        //
        //       return Redirect("/");
        //   }

        //   [NonAction]
        //   internal async Task<UserAccount> GetUserAccountFromCoockyValue()
        //   {
        //       if (_httpContextAccessor.HttpContext.Request.Cookies
        //           .TryGetValue("ConfirmUserAccountId", out string userIdStr))
        //       {
        //           if (Guid.TryParse(userIdStr, out Guid userId))
        //           {
        //               return await _userAccountService.LoadByIdAsync(userId);
        //           }
        //       }
        //
        //       return null;
        //   }

        [NonAction]
        internal async Task<RegisterViewModel> CreateViewModelAsync(string returnUrl)
        {
            return await CreateViewModelAsync(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [NonAction]
        internal async Task<RegisterViewModel> CreateViewModelAsync(
            RegisterInputModel inputModel,
            UserAccount userAccount = null)
        {
            var context = await _interaction.GetAuthorizationContextAsync(inputModel.ReturnUrl);
            if (context == null)
            {
                return null;
            }

            var client = await _clientService.FindEnabledClientByIdAsync(context.ClientId);
            var providers = await _clientService.GetEnabledProvidersAsync(client);

            var vm = new RegisterViewModel(inputModel)
            {
                EnableAccountRecover = _applicationOptions.EnableAccountRecover,
                EnableLocalLogin = (client != null ? client.EnableLocalLogin : false) &&
                    _applicationOptions.EnableLocalLogin,
                ExternalProviders = providers.ToArray(),
                ExternalProviderHints = userAccount?.Accounts?.Select(c => c.Provider)
            };

            return vm;
        }

        [NonAction]
        internal SuccessViewModel CreateSuccessViewModel(UserAccount userAccount, string returnUrl)
        {
            return new SuccessViewModel()
            {
                ReturnUrl = returnUrl,
                Provider = userAccount.Email.Split('@').LastOrDefault()
            };
        }

        [NonAction]
        internal async Task<IActionResult> TryMergeWithExistingUserAccount(
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
                    return View("Success", CreateSuccessViewModel(userAccount, model.ReturnUrl)); 
                }
            }
            // Ask user if he wants to merge accounts
            else
            {
                throw new NotImplementedException();
            }

            // Return list of external account providers as hint
            var vm = new RegisterViewModel(model)
            {
                ExternalProviderHints = userAccount.Accounts.Select(s => s.Provider).ToArray()
            };
            return View(vm);
        }

        [NonAction]
        internal async Task SendEmailAsync(UserAccount userAccount)
        {
            var baseUrl = ServiceBase.Extensions.StringExtensions
                .EnsureTrailingSlash(_httpContextAccessor.HttpContext.GetIdentityServerBaseUrl());

            await _emailService.SendEmailAsync(IdentityBaseConstants.EmailTemplates
                .UserAccountCreated, userAccount.Email, new
                {
                    ConfirmUrl = $"{baseUrl}register/confirm/{userAccount.VerificationKey}",
                    CancelUrl = $"{baseUrl}register/cancel/{userAccount.VerificationKey}"
                }, true);
        }

        [NonAction]
        internal async Task<IActionResult> TryCreateNewUserAccount(
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

            return View("Success", CreateSuccessViewModel(userAccount, model.ReturnUrl));
        }
    }
}