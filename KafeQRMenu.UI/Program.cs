using KafeQRMenu.BLogic.Extensions;
using KafeQRMenu.DataAccess.Extensions;
using KafeQRMenu.UI.Extensions;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddBLogicServices();
builder.Services.AddUIServices();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // RememberMe için
    options.SlidingExpiration = true;
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
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Service Worker ve Manifest için doğru MIME types
        if (ctx.File.Name == "service-worker.js")
        {
            ctx.Context.Response.Headers.Append("Service-Worker-Allowed", "/");
            ctx.Context.Response.Headers.Append("Content-Type", "application/javascript");
        }
        else if (ctx.File.Name == "manifest.json")
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/manifest+json");
        }

        // Cache control
        const int durationInSeconds = 60 * 60 * 24 * 7; // 7 days
        ctx.Context.Response.Headers[HeaderNames.CacheControl] =
            "public,max-age=" + durationInSeconds;
    }
});

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapDefaultControllerRoute();

app.Run();
