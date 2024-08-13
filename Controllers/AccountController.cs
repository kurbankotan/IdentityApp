using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace IdentityApp.Controllers
{
    public class AccountController:Controller
    {
        private UserManager<AppUser> _userManager;
        private RoleManager<AppRole> _roleManager;
        private SignInManager<AppUser> _signInManager;
        private IEmailSender _emailSender;
        private readonly IConfiguration _configuration; // IConfiguration nesnesini tanımlayalım
        public AccountController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
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

                    if(!await _userManager.IsEmailConfirmedAsync(user))  //Hesap onaysız ise login sayfasını kullanıcıya tekrar gönder
                    {
                        ModelState.AddModelError("", "Hesabınızı onaylayınız");
                        return View(model);
                    }

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




        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var hostUrl = _configuration.GetValue<string>("AppHost:Url");
                    var url = Url.Action("ConfirmEmail", "Account", new { Id = user.Id, token = token }, protocol: hostUrl);

                    //email servisi
                    //href kısmınıza IdentityApp'in adresini yazın


                    await _emailSender.SendEmailAsync(user.Email, "Hesap Onayı", $"Lütfen eposta hesabınızı onaylamak için linke <a href='{url}'>tıklayınız.</a>");

                    TempData["message"] = "Email Hesabınızdaki Onay Mailini Tıklayınız";
                    return RedirectToAction("Login", "Account");
                }

                foreach (IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
            return View(model);
        }



        public async Task<IActionResult> ConfirmEmail(string Id, string token)
        {
            if(Id == null || token == null)
            {
                TempData["message"] = "Geçersiz token bilgisi";
                return View();
            }

            var user = await _userManager.FindByIdAsync(Id);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user,token);
                if (result.Succeeded)
                {
                    TempData["message"] = "Hesabınız Onaylandı";
                    return RedirectToAction("Login", "Account");
                }
            }

            TempData["message"] = "Kullanıcı Bulunamadı";
            return View();

        }






    }
}
