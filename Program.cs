using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IdentityContext>(
    options => options.UseSqlite(builder.Configuration["ConnectionStrings:sql_connection"]));


builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<IdentityContext>();   //AppUser yerine daha �nce IdentityUser vard�. IdentityUser Microsoft'un tan�mlad���
                                                                                                //AppRole yerine daha �nce IdentityRole vard�. IdentityRole Microsoft'un tan�mlad���

//Identiy'nin ayarlar�n� de�i�tirmek i�in
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); //Yanl�� �ifre girdikten sonra hesap kitlenip, kullan�c� 5 dakika bekletilir yeni bir login i�lemi i�in
    options.Lockout.MaxFailedAccessAttempts = 3; //3 tane arka arkaya yanl�� girilirse hesap kitlenir

});


builder.Services.ConfigureApplicationCookie(options =>{

    options.LoginPath = "/Account/Login"; // "/Users/Login" de olu�turulup y�neledirilebilir. Bu zaten default, yaz�lmasa da olur
    // options.LogoutPath = "/";  //Logout sonras� gidilecek sayfa
    options.AccessDeniedPath = "/Account/AccessDenied";  //Uygulamaya giri� yap�ld� ama girilen yerde yetkin yok uyars� i�in bir sayfaya y�nlendirme
    options.ExpireTimeSpan = TimeSpan.FromDays(30); //Cookie'nin expire s�resi. Defatul 14 g�n. FromMinutes(30) yap�l�rsa 30 dakika olur
    options.SlidingExpiration = true;  // 15. g�n tekrar login olursa cookie'nin expire s�resi yukar�da tan�mland��� gibi 30 g�n olur tekrardan
    
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
