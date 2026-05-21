using Microsoft.EntityFrameworkCore;
using QuanLyThuChiWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// ??NG KÝ SQL SERVER K?T N?I T?I ?ÂY
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DÒNG TH? 1 ???C THÊM T?I ?ÂY: ??ng ký d?ch v? Session vào Container
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Th?i gian Session t?n t?i (30 phút)
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

// ?? dùng ???c @inject IHttpContextAccessor trong file _LoginPartial.cshtml
builder.Services.AddHttpContextAccessor();

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

// DÒNG TH? 2 ???C THÊM T?I ?ÂY: Kích ho?t Middleware Session (B?t bu?c ph?i n?m TR??C UseAuthorization)
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
