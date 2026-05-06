using Chat_Ai.DTOs;
using Chat_Ai.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Chat_Ai.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message!);
                return View(dto);
            }

            // Registration successful — sign the user in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId!),
                new Claim(ClaimTypes.Name, dto.Name),
                new Claim(ClaimTypes.Email, dto.Email)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ExternalLogin(string provider = "Google")
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, provider);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync("Cookies");
            if (result.Succeeded)
            {
                // In a real app, you'd save the user to your DB here
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
