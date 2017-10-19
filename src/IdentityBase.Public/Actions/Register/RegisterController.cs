namespace IdentityBase.Public.Actions.Register
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
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;

    public class RegisterController : Controller
    {
        private readonly ApplicationOptions applicationOptions;
        private readonly ILogger<RegisterController> logger;
        private readonly IIdentityServerInteractionService interaction;
        private readonly IEmailService emailService;
        private readonly UserAccountService userAccountService;
        private readonly ClientService clientService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public RegisterController(
            ApplicationOptions applicationOptions,
            ILogger<RegisterController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            ClientService clientService,
            IHttpContextAccessor httpContextAccessor)
        {
            this.applicationOptions = applicationOptions;
            this.logger = logger;
            this.interaction = interaction;
            this.emailService = emailService;
            this.userAccountService = userAccountService;
            this.clientService = clientService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("register", Name = "Register")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var vm = await this.CreateViewModelAsync(returnUrl);
            if (vm == null)
            {
                logger.LogError("Register attempt with missing returnUrl parameter");
                return Redirect(Url.Action("Index", "Error"));
            }

            return View(vm);
        }

        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterInputModel model)
        {
            if (!ModelState.IsValid)
            {
                return this.View(await this.CreateViewModelAsync(model));
            }

            var email = model.Email.ToLower();

            // Check if user with same email exists
            var userAccount = await userAccountService
                .LoadByEmailWithExternalAsync(email);

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
                if (applicationOptions.RequireLocalAccountVerification &&
                    !userAccount.IsEmailVerified)
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

        [HttpGet("register/confirm/{key}", Name = "RegisterConfirm")]
        public async Task<IActionResult> Confirm(string key)
        {
            var result = await userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            // User account requires completion 
            if (applicationOptions.EnableInvitationCreateEndpoint &&
                result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                var vm = new ConfirmViewModel
                {
                    Key = key,
                    RequiresPassword = !result.UserAccount.HasPassword(),
                    Email = result.UserAccount.Email
                };

                return View(vm);
            }
            // User profile already fine and just needs to be activated
            else
            {
                var returnUrl = result.UserAccount.VerificationKey;
                await userAccountService.SetEmailVerifiedAsync(result.UserAccount);

                if (applicationOptions.LoginAfterAccountConfirmation)
                {
                    await httpContextAccessor.HttpContext.SignInAsync(result.UserAccount, null);

                    if (returnUrl != null && interaction.IsValidReturnUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                }

                return Redirect(Url.Action("Login", "Login", new { ReturnUrl = returnUrl }));
            }
        }

        // Currently is only used for invitations 
        [HttpPost("register/confirm/{key}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmInputModel model)
        {
            if (!this.applicationOptions.EnableInvitationCreateEndpoint)
            {
                return NotFound();
            }

            var result = await userAccountService.HandleVerificationKeyAsync(model.Key,
               VerificationKeyPurpose.ConfirmAccount);

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
            userAccountService.SetEmailVerified(result.UserAccount);
            userAccountService.AddLocalCredentials(result.UserAccount, model.Password);
            await userAccountService.UpdateUserAccountAsync(result.UserAccount);

            if (result.UserAccount.CreationKind == CreationKind.Invitation)
            {
                // TODO: validate 
                return Redirect(returnUrl);
            }
            else
            {
                if (applicationOptions.LoginAfterAccountRecovery)
                {
                    await httpContextAccessor.HttpContext.SignInAsync(result.UserAccount, null);

                    if (interaction.IsValidReturnUrl(returnUrl))
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
            var result = await userAccountService.HandleVerificationKeyAsync(key,
                VerificationKeyPurpose.ConfirmAccount);

            if (result.UserAccount == null || !result.PurposeValid || result.TokenExpired)
            {
                // TODO: clear token if account is there 

                ModelState.AddModelError(IdentityBaseConstants.ErrorMessages.TokenIsInvalid);
                return View("InvalidToken");
            }

            var returnUrl = result.UserAccount.VerificationStorage;
            await userAccountService.ClearVerificationAsync(result.UserAccount);


            if (interaction.IsValidReturnUrl(returnUrl))
            {
                return Redirect(Url.Action("Index", "Login", new { ReturnUrl = returnUrl }));
            }
            else
            {
                return Redirect(returnUrl);
            }
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
        //       httpContextAccessor.HttpContext.Response.Cookies.Delete("ConfirmUserAccountId");
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
        //           userAccountService.AddLocalCredentialsAsync(userAccount, model.Password),
        //           httpContextAccessor.HttpContext.Authentication.SignInAsync(userAccount, null)
        //       ); */
        //
        //       // && interaction.IsValidReturnUrl(returnUrl)
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
        //       if (httpContextAccessor.HttpContext.Request.Cookies
        //           .TryGetValue("ConfirmUserAccountId", out string userIdStr))
        //       {
        //           if (Guid.TryParse(userIdStr, out Guid userId))
        //           {
        //               return await userAccountService.LoadByIdAsync(userId);
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
            var context = await interaction.GetAuthorizationContextAsync(inputModel.ReturnUrl);
            if (context == null)
            {
                return null;
            }

            var client = await clientService.FindEnabledClientByIdAsync(context.ClientId);
            var providers = await clientService.GetEnabledProvidersAsync(client);

            var vm = new RegisterViewModel(inputModel)
            {
                EnableAccountRecover = applicationOptions.EnableAccountRecovery,
                EnableLocalLogin = (client != null ? client.EnableLocalLogin : false) &&
                    applicationOptions.EnableLocalLogin,
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
            if (applicationOptions.AutomaticAccountMerge)
            {
                await userAccountService.AddLocalCredentialsAsync(userAccount, model.Password);

                if (applicationOptions.LoginAfterAccountCreation)
                {
                    await httpContextAccessor.HttpContext.SignInAsync(userAccount, null);

                    if (model.ReturnUrl != null && interaction.IsValidReturnUrl(model.ReturnUrl))
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
                .EnsureTrailingSlash(httpContextAccessor.HttpContext.GetIdentityServerBaseUrl());

            await emailService.SendEmailAsync(IdentityBaseConstants.EmailTemplates
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
            userAccount = await userAccountService.CreateNewLocalUserAccountAsync(
                        model.Email, model.Password, model.ReturnUrl);

            // Send confirmation mail
            if (applicationOptions.RequireLocalAccountVerification)
            {
                await SendEmailAsync(userAccount);
            }

            if (applicationOptions.LoginAfterAccountCreation)
            {
                await httpContextAccessor.HttpContext.SignInAsync(userAccount, null);

                if (model.ReturnUrl != null && interaction.IsValidReturnUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
            }

            return View("Success", CreateSuccessViewModel(userAccount, model.ReturnUrl));
        }
    }
}