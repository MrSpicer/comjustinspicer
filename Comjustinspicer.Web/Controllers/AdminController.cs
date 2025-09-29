using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;

namespace comjustinspicer.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AdminController(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public IActionResult ResendConfirmation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return View();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return View();

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

            ViewBag.Message = "Confirmation email sent (dev log contains the link).";
            return View();
        }
    }
}
