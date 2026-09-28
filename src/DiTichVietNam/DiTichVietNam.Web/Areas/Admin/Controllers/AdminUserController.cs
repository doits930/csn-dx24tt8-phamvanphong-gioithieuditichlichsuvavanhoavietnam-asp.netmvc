using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Services.Accounts;
using DiTichVietNam.Web.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class AdminUserController : AdminBaseController
{
    public const string ListPath = "/admin/tai-khoan-quan-tri";
    public const string CreatePath = "/admin/tai-khoan-quan-tri/them";
    public const string LockPath = "/admin/tai-khoan-quan-tri/{id}/khoa";
    public const string UnlockPath = "/admin/tai-khoan-quan-tri/{id}/mo-khoa";
    public const string ResetPasswordPath = "/admin/tai-khoan-quan-tri/{id}/dat-lai-mat-khau";
    public const string DeletePath = "/admin/tai-khoan-quan-tri/{id}/xoa";
    public const string ChangePasswordPath = "/admin/doi-mat-khau";

    private const string CreateTitle = "Thêm tài khoản quản trị";
    private const string ResetTitle = "Đặt lại mật khẩu";
    private const string ChangeTitle = "Đổi mật khẩu";

    private readonly IAdminUserService _adminUserService;
    private readonly IAccountService _accountService;
    private readonly PasswordPolicy _passwordPolicy;
    private readonly AccountNamePolicy _namePolicy;

    public AdminUserController(
        IAdminUserService adminUserService,
        IAccountService accountService,
        PasswordPolicy passwordPolicy,
        AccountNamePolicy namePolicy)
    {
        _adminUserService = adminUserService;
        _accountService = accountService;
        _passwordPolicy = passwordPolicy;
        _namePolicy = namePolicy;
    }

    public static string LockUrl(string id) => $"/admin/tai-khoan-quan-tri/{Uri.EscapeDataString(id)}/khoa";

    public static string UnlockUrl(string id) => $"/admin/tai-khoan-quan-tri/{Uri.EscapeDataString(id)}/mo-khoa";

    public static string ResetPasswordUrl(string id) =>
        $"/admin/tai-khoan-quan-tri/{Uri.EscapeDataString(id)}/dat-lai-mat-khau";

    public static string DeleteUrl(string id) => $"/admin/tai-khoan-quan-tri/{Uri.EscapeDataString(id)}/xoa";

    [HttpGet(ListPath)]
    public async Task<IActionResult> Index()
    {
        var result = await _adminUserService.GetListAsync(CurrentUserId);

        ViewData["Title"] = "Quản lý tài khoản quản trị";

        return View(AdminUserViewFactory.ToListVM(result));
    }

    [HttpGet(CreatePath)]
    public IActionResult Create([FromQuery] string? returnUrl) =>
        ShowCreateForm(new AdminUserFormVM { ReturnUrl = SafeListReturnUrl(returnUrl, ListPath) });

    [HttpPost(CreatePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] AdminUserFormVM vm)
    {
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = AdminUserViewFactory.ToInput(vm);
        AddErrors(AdminUserValidator.ValidateCreate(input, _passwordPolicy, _namePolicy));

        if (ModelState.IsValid)
        {
            var result = await _adminUserService.CreateAsync(input, CurrentUserId);
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã thêm tài khoản {input.Email?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            if (IsActorRejected(result))
            {
                return await SignOutRejectedActorAsync();
            }

            AddServiceError(result);
        }

        return ShowCreateForm(vm);
    }

    [HttpGet(LockPath)]
    public async Task<IActionResult> ConfirmLock([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var row = await LoadRowAsync(id);
        if (row is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Khóa tài khoản";

        return View("Confirm", new AdminUserConfirmVM
        {
            Title = "Khóa tài khoản",
            Lead = $"Khóa tài khoản {row.Email}.",
            Message = "Tài khoản bị khóa không đăng nhập được, và phiên đang mở của tài khoản đó mất hiệu lực ngay ở yêu cầu kế tiếp.",
            ActionUrl = LockUrl(row.Id),
            ConfirmLabel = "Khóa tài khoản",
            CancelUrl = SafeListReturnUrl(returnUrl, ListPath) ?? ListPath,
            IsDangerous = true,
            BlockReason = row.LockBlockReason
        });
    }

    [HttpPost(LockPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var result = await _adminUserService.LockAsync(id, CurrentUserId);

        return await FinishAsync(result, email => $"Đã khóa tài khoản {email}.", returnUrl);
    }

    [HttpPost(UnlockPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var result = await _adminUserService.UnlockAsync(id, CurrentUserId);

        return await FinishAsync(result, email => $"Đã mở khóa tài khoản {email}.", returnUrl);
    }

    [HttpGet(ResetPasswordPath)]
    public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var row = await LoadRowAsync(id);
        if (row is null)
        {
            return AdminNotFound();
        }

        if (row.ResetBlockReason is not null)
        {
            TempData["AdminError"] = row.ResetBlockReason;

            return Redirect(ListPath);
        }

        return ShowResetForm(new AdminUserPasswordVM
        {
            Id = row.Id,
            Email = row.Email,
            IsLocked = row.Status == AdminUserStatus.Locked,
            ReturnUrl = SafeListReturnUrl(returnUrl, ListPath)
        });
    }

    [HttpPost(ResetPasswordPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromForm] AdminUserPasswordVM vm)
    {
        var row = await LoadRowAsync(id);
        if (row is null)
        {
            return AdminNotFound();
        }

        vm.Id = row.Id;
        vm.Email = row.Email;
        vm.IsLocked = row.Status == AdminUserStatus.Locked;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = AdminUserViewFactory.ToInput(vm);
        AddErrors(AdminUserValidator.ValidateReset(input, _passwordPolicy));

        if (ModelState.IsValid)
        {
            var result = await _adminUserService.ResetPasswordAsync(row.Id, input, CurrentUserId);
            if (result.Success)
            {
                SetResetSuccessNotice(result.Data ?? row.Email, vm.IsLocked);

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            if (IsActorRejected(result))
            {
                return await SignOutRejectedActorAsync();
            }

            AddServiceError(result);
        }

        return ShowResetForm(vm);
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var row = await LoadRowAsync(id);
        if (row is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Xóa tài khoản";

        return View("Confirm", new AdminUserConfirmVM
        {
            Title = "Xóa tài khoản",
            Lead = $"Xóa tài khoản {row.Email} khỏi hệ thống.",
            Message = "Tài khoản bị xóa không đăng nhập lại được và thao tác này không lấy lại được.",
            ActionUrl = DeleteUrl(row.Id),
            ConfirmLabel = "Xóa tài khoản",
            CancelUrl = SafeListReturnUrl(returnUrl, ListPath) ?? ListPath,
            IsDangerous = true,
            BlockReason = row.DeleteBlockReason
        });
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] string id, [FromQuery] string? returnUrl)
    {
        var result = await _adminUserService.DeleteAsync(id, CurrentUserId);

        return await FinishAsync(result, email => $"Đã xóa tài khoản {email}.", returnUrl);
    }

    [HttpGet(ChangePasswordPath)]
    public IActionResult ChangePassword() => ShowChangeForm(new ChangePasswordVM());

    [HttpPost(ChangePasswordPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordVM vm)
    {
        var input = AdminUserViewFactory.ToInput(vm);
        AddErrors(AdminUserValidator.ValidateChange(input, _passwordPolicy));

        if (ModelState.IsValid)
        {
            var result = await _adminUserService.ChangeOwnPasswordAsync(CurrentUserId, input);
            if (result.Success)
            {
                TempData["AdminSuccess"] = "Đã đổi mật khẩu. Những phiên đăng nhập khác của bạn đã bị đăng xuất.";

                return Redirect(ListPath);
            }

            if (result.Data == PasswordChangeStatus.LockedOut)
            {
                return Redirect(AccountController.LoginPath);
            }

            AddServiceError(result);
        }

        return ShowChangeForm(vm);
    }

    private string CurrentUserId => _accountService.GetUserId(User) ?? string.Empty;

    private static bool IsActorRejected(ServiceResult result)
        => !result.Success && result.Error == AdminUserService.ActorRejectedMessage;

    private async Task<IActionResult> SignOutRejectedActorAsync()
    {
        await _accountService.SignOutAsync(User);

        return Redirect(AccountController.LoginPath);
    }

    private void SetResetSuccessNotice(string email, bool isLocked)
    {
        if (!isLocked)
        {
            TempData["AdminSuccess"] = $"Đã đặt lại mật khẩu cho tài khoản {email}.";

            return;
        }

        TempData["AdminSuccess"] =
            $"Đã đặt lại mật khẩu cho tài khoản {email}. Tài khoản này vẫn đang bị khóa nên chưa đăng nhập được.";
        TempData["AdminAlertLinkUrl"] = ListPath;
        TempData["AdminAlertLinkText"] = "Mở khóa tài khoản ở danh sách";
    }

    private async Task<AdminUserRowVM?> LoadRowAsync(string id)
    {
        var row = await _adminUserService.GetRowAsync(id, CurrentUserId);

        return row is null
            ? null
            : AdminUserViewFactory.ToRowVM(row, await _adminUserService.CountUsableAsync());
    }

    private async Task<IActionResult> FinishAsync(
        ServiceResult<string> result,
        Func<string, string> successMessage,
        string? returnUrl)
    {
        if (result.Success)
        {
            TempData["AdminSuccess"] = successMessage(result.Data ?? string.Empty);
        }
        else if (IsActorRejected(result))
        {
            return await SignOutRejectedActorAsync();
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(SafeListReturnUrl(returnUrl, ListPath) ?? ListPath);
    }

    private IActionResult ShowCreateForm(AdminUserFormVM vm)
    {
        vm.Password = null;
        vm.ConfirmPassword = null;
        vm.PasswordRules = _passwordPolicy.Rules;
        vm.AllowedEmailCharacters = _namePolicy.AllowedCharacters;
        ViewData["Title"] = CreateTitle;

        return View("Form", vm);
    }

    private IActionResult ShowResetForm(AdminUserPasswordVM vm)
    {
        vm.Password = null;
        vm.ConfirmPassword = null;
        vm.PasswordRules = _passwordPolicy.Rules;
        ViewData["Title"] = ResetTitle;

        return View("ResetPassword", vm);
    }

    private IActionResult ShowChangeForm(ChangePasswordVM vm)
    {
        vm.CurrentPassword = null;
        vm.NewPassword = null;
        vm.ConfirmPassword = null;
        vm.AccountEmail = _accountService.GetDisplayName(User) ?? string.Empty;
        vm.PasswordRules = _passwordPolicy.Rules;
        ViewData["Title"] = ChangeTitle;

        return View("ChangePassword", vm);
    }
}
