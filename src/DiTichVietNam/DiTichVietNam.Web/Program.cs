using DiTichVietNam.Web.Areas.Admin.Controllers;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Middleware;
using DiTichVietNam.Web.Services.Accounts;
using DiTichVietNam.Web.Services.Images;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=Data/DiTichVietNam.db";

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IRelicService, RelicService>();
builder.Services.AddScoped<IProvinceService, ProvinceService>();
builder.Services.AddScoped<IRelicTypeService, RelicTypeService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRelicImageService, RelicImageService>();
builder.Services.AddSingleton<RelicImageWriteLock>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = ImageUploadLimits.MaxRequestBytes;
});

builder.Services.Configure<AdminSiteOptions>(builder.Configuration.GetSection(AdminSiteOptions.SectionName));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/tai-khoan/dang-nhap";
    options.AccessDeniedPath = "/tai-khoan/dang-nhap";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

var adminSite = app.Services.GetRequiredService<IOptions<AdminSiteOptions>>().Value;

if (!app.Environment.IsDevelopment())
{
    app.UseWhen(
        context => context.Connection.LocalPort == adminSite.Port,
        branch => branch.UseExceptionHandler(AdminHomeController.ErrorPath));

    app.UseWhen(
        context => context.Connection.LocalPort != adminSite.Port,
        branch => branch.UseExceptionHandler("/Home/Error"));

    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/loi/{0}");
app.UseMiddleware<AdminPortIsolationMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

var uploadRoot = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "uploads");
Directory.CreateDirectory(uploadRoot);

var uploadContentTypes = new FileExtensionContentTypeProvider();
uploadContentTypes.Mappings.Clear();
uploadContentTypes.Mappings[".jpg"] = "image/jpeg";
uploadContentTypes.Mappings[".jpeg"] = "image/jpeg";
uploadContentTypes.Mappings[".png"] = "image/png";
uploadContentTypes.Mappings[".webp"] = "image/webp";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadRoot),
    RequestPath = "/uploads",
    ContentTypeProvider = uploadContentTypes,
    ServeUnknownFileTypes = false,
    OnPrepareResponse = context =>
        context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff"
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var databasePath = Path.Combine(app.Environment.ContentRootPath, "Data");
    Directory.CreateDirectory(databasePath);

    await context.Database.MigrateAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider);
}

AdminSiteStartupCheck.Register(app);

app.Run();
