using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Areas.Admin.Validation;
using DiTichVietNam.Web.Services.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

[Area("Admin")]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class AccountController : Controller
{
    public const string AdminHomePath = DashboardController.IndexPath;
    public const string LoginPath = "/tai-khoan/dang-nhap";
    public const string LogoutPath = "/tai-khoan/dang-xuat";

    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet(LoginPath)]
    public IActionResult Login(string? returnUrl)
    {
        if (_accountService.IsAdminSignedIn(User))
        {
            return Redirect(AdminHomePath);
        }

        PrepareLoginView();

        return View(new LoginVM { ReturnUrl = ToLocalUrl(returnUrl) });
    }

    [HttpPost(LoginPath)]
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
            var result = await _accountService.SignInAsync(vm.UserName, vm.Password);
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

    [HttpPost(LogoutPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutAsync();

        return Redirect(LoginPath);
    }

    private void PrepareLoginView() => ViewData["Title"] = "Đăng nhập quản trị";

    private string? ToLocalUrl(string? returnUrl)
        => !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : null;
}
