using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApp.Controllers
{
    public class AccountController:Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    await _signInManager.SignOutAsync(); //önce açık bir sigin varsa onu signout yapalım
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe,true);

                    if (result.Succeeded)
                    {
                        await _userManager.ResetAccessFailedCountAsync(user);  //Giriş yapıldıktan sonra eğer hesap kitleme değerlerini sıfırla
                        await _userManager.SetLockoutEndDateAsync(user, null);

                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)  //Yine hatalı giriş olursa
                    {
                        var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                        var timeLeft = lockoutDate.Value - DateTime.UtcNow;
                        ModelState.AddModelError("", $"Hesabınız kitlendi, lütfen {timeLeft.Minutes} dakika sonra deneyiniz.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Parolanız Hatalı.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu eposta adresiyle bir hesap bulunamadı.");
                }
            }
            return View(model);  //Hatalı ise modeli sayfaya geri gönder
        }
    }
}
