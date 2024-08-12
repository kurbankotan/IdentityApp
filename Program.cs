using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IdentityContext>(
    options => options.UseSqlite(builder.Configuration["ConnectionStrings:sql_connection"]));


builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<IdentityContext>();   //AppUser yerine daha önce IdentityUser vardý. IdentityUser Microsoft'un tanýmladýðý
                                                                                                //AppRole yerine daha önce IdentityRole vardý. IdentityRole Microsoft'un tanýmladýðý

//Identiy'nin ayarlarýný deðiþtirmek için
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //Yanlýþ þifre girdikten sonra hesap kitlenip, kullanýcý 5 dakika bekletilir yeni bir login iþlemi için
    options.Lockout.MaxFailedAccessAttempts = 3; //3 tane arka arkaya yanlýþ girilirse hesap kitlenir

});


builder.Services.ConfigureApplicationCookie(options =>{

    options.LoginPath = "/Account/Login"; // "/Users/Login" de oluþturulup yöneledirilebilir. Bu zaten default, yazýlmasa da olur
    // options.LogoutPath = "/";  //Logout sonrasý gidilecek sayfa
    options.AccessDeniedPath = "/Account/AccessDenied";  //Uygulamaya giriþ yapýldý ama girilen yerde yetkin yok uyarsý için bir sayfaya yönlendirme
    options.ExpireTimeSpan = TimeSpan.FromDays(30); //Cookie'nin expire süresi. Defatul 14 gün. FromMinutes(30) yapýlýrsa 30 dakika olur
    options.SlidingExpiration = true;  // 15. gün tekrar login olursa cookie'nin expire süresi yukarýda tanýmlandýðý gibi 30 gün olur tekrardan
    
}); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



IdentitySeedData.IdentityTestUser(app);

app.Run();
