using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Controllers;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class AccountController : Controller
{
    public const string AdminHomePath = "/admin/di-tich";

    private readonly IAccountService _accountService;
    private readonly AdminSiteOptions _adminSite;

    public AccountController(IAccountService accountService, IOptions<AdminSiteOptions> adminSite)
    {
        _accountService = accountService;
        _adminSite = adminSite.Value;
    }

    [HttpGet("/tai-khoan/dang-nhap")]
    public IActionResult Login(string? returnUrl)
    {
        if (_accountService.IsAdminSignedIn(User))
        {
            return Redirect(AdminHomePath);
        }

        PrepareLoginView();

        return View(new LoginVM { ReturnUrl = ToLocalUrl(returnUrl) });
    }

    [HttpPost("/tai-khoan/dang-nhap")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM vm)
    {
        vm.ReturnUrl = ToLocalUrl(vm.ReturnUrl);

        foreach (var (field, message) in LoginValidator.Validate(vm))
        {
            ModelState.AddModelError(field, message);
        }

        if (ModelState.IsValid)
        {
            var result = await _accountService.SignInAsync(vm);
            if (result.Success)
            {
                return Redirect(vm.ReturnUrl ?? AdminHomePath);
            }

            ViewData["LoginAlert"] = result.Error;
            ViewData["LoginAlertTone"] = result.Data == LoginStatus.LockedOut ? "warning" : "error";
        }

        vm.Password = null;
        PrepareLoginView();

        return View(vm);
    }

    [HttpPost("/tai-khoan/dang-xuat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutAsync();

        return Redirect(SiteUrls.OnPort(Request, _adminSite.PublicPort, "/"));
    }

    private void PrepareLoginView()
    {
        ViewData["Title"] = "Đăng nhập quản trị";
        ViewData["HideNavSearch"] = true;
    }

    private string? ToLocalUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;
}
