// Copyright (c) Russlan Akiev. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityBase.Actions.Register
{
    using IdentityBase.Configuration;
    using IdentityBase.Services;
    using IdentityServer4.Services;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;
    using ServiceBase.Notification.Email;

    public class RegisterCompleteController : WebController
    {
        private readonly ApplicationOptions _applicationOptions;
        private readonly ILogger<RegisterCompleteController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly UserAccountService _userAccountService;
        private readonly ClientService _clientService;
        private readonly NotificationService _notificationService;
        private readonly AuthenticationService _authenticationService;
        private readonly IStringLocalizer _localizer;

        public RegisterCompleteController(
            ApplicationOptions applicationOptions,
            ILogger<RegisterCompleteController> logger,
            IIdentityServerInteractionService interaction,
            IEmailService emailService,
            UserAccountService userAccountService,
            ClientService clientService,
            NotificationService notificationService,
            AuthenticationService authenticationService,
            IStringLocalizer localizer)
        {
            this._applicationOptions = applicationOptions;
            this._logger = logger;
            this._interaction = interaction;
            this._userAccountService = userAccountService;
            this._clientService = clientService;
            this._notificationService = notificationService;
            this._authenticationService = authenticationService;
            this._localizer = localizer;
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
    }
}