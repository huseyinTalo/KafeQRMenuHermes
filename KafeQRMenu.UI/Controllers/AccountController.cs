using KafeQRMenu.UI.Models.AccountVMs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace KafeQRMenu.UI.Controllers
{
    [AllowAnonymous]
    [Route("Account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [Route("Login")]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Zaten giriş yapmışsa yönlendir
            if (User.Identity.IsAuthenticated)
            {
                return await RedirectToRoleBasedAreaAsync();
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginVM loginVM, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(loginVM);
            }

            var result = await _signInManager.PasswordSignInAsync(
                loginVM.Email,
                loginVM.Password,
                loginVM.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(loginVM.Email);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    // ReturnUrl varsa ve güvenliyse oraya yönlendir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    // Rol bazlı yönlendirme
                    if (roles.Contains("SuperAdmin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "SuperAdmin" });
                    }
                    else if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    }
                    else
                    {
                        // Rolü yoksa çıkış yap ve hata ver
                        await _signInManager.SignOutAsync();
                        ModelState.AddModelError(string.Empty, "Bu hesabın yönetim paneline erişim yetkisi bulunmuyor.");
                        return View(loginVM);
                    }
                }
            }

            ModelState.AddModelError(string.Empty, "Email veya şifre hatalı.");
            return View(loginVM);
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [Route("AccessDenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Rol bazlı yönlendirme helper metodu
        private async Task<IActionResult> RedirectToRoleBasedAreaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("SuperAdmin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "SuperAdmin" });
                }
                else if (roles.Contains("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
            }

            return RedirectToAction("Index", "Home");
        }
    }
}