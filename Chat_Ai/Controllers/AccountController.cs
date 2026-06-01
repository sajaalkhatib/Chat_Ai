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
        private readonly IConfiguration _configuration;

        public AccountController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message!);
                return View(dto);
            }

            // Login successful — sign the user in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.UserId!),
                new Claim(ClaimTypes.Name, result.Message!), // Message contains user name
                new Claim(ClaimTypes.Email, dto.Email)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
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
            var clientId = _configuration["Authentication:Google:ClientId"];
            if (string.IsNullOrEmpty(clientId) || clientId.Contains("YOUR_GOOGLE_CLIENT_ID") || clientId.Contains("اكتب_هنا"))
            {
                return RedirectToAction("MockGoogleLogin");
            }

            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, provider);
        }

        [HttpGet]
        public IActionResult MockGoogleLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MockGoogleLogin(string email, string name)
        {
            if (string.IsNullOrEmpty(email))
            {
                email = "sjalkhatib@gmail.com";
            }
            if (string.IsNullOrEmpty(name))
            {
                name = "Saja Alkhatib";
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "mock-google-id-12345"),
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Email, email)
            };

            var identity = new ClaimsIdentity(claims, "External");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("External", principal);

            return RedirectToAction("GoogleResponse");
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync("External");
            if (!result.Succeeded || result.Principal == null)
            {
                return RedirectToAction("Login");
            }

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                await HttpContext.SignOutAsync("External");
                return RedirectToAction("Login");
            }

            var name = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;

            // Get or create the user in the database
            var user = await _authService.GetOrCreateExternalUserAsync(email, name);

            // Clean up the temporary external cookie
            await HttpContext.SignOutAsync("External");

            // Sign the user in with the database claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index", "Home");
        }
    }
}
 