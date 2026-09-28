using System.Security.Claims;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Services.Common;
using Microsoft.AspNetCore.Identity;

namespace DiTichVietNam.Web.Services.Accounts;

public class AccountService : IAccountService
{
    public const string GenericFailureMessage = "Tài khoản hoặc mật khẩu không đúng.";

    public const string DisabledAccountMessage =
        "Tài khoản này đang bị khóa. Liên hệ người quản trị khác để mở khóa.";

    private const string DecoyPassword = "decoy-password-for-constant-time";

    private static string? _decoyHash;

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<ServiceResult<LoginStatus>> SignInAsync(string? userName, string? password)
    {
        var trimmedUserName = userName?.Trim();
        if (string.IsNullOrEmpty(trimmedUserName) || string.IsNullOrEmpty(password))
        {
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        var user = await _userManager.FindByNameAsync(trimmedUserName);
        if (user is null)
        {
            SpendHashingTime(password);
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        if (AdminUserFactory.IsIndefiniteLock(user.LockoutEnd))
        {
            var knowsPassword = await _userManager.CheckPasswordAsync(user, password);

            return knowsPassword
                ? ServiceResult<LoginStatus>.Fail(DisabledAccountMessage, LoginStatus.LockedOut)
                : ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        var attempt = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);

        if (attempt.IsLockedOut)
        {
            return ServiceResult<LoginStatus>.Fail(await BuildLockoutMessageAsync(trimmedUserName), LoginStatus.LockedOut);
        }

        if (!attempt.Succeeded)
        {
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        if (!await _userManager.IsInRoleAsync(user, SeedData.AdminRoleName))
        {
            await _signInManager.SignOutAsync();
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.Forbidden);
        }

        return ServiceResult<LoginStatus>.Ok(LoginStatus.Success);
    }

    private void SpendHashingTime(string password)
    {
        var decoyUser = new IdentityUser();
        _decoyHash ??= _userManager.PasswordHasher.HashPassword(decoyUser, DecoyPassword);
        _userManager.PasswordHasher.VerifyHashedPassword(decoyUser, _decoyHash, password);
    }

    public async Task SignOutAsync(ClaimsPrincipal principal)
    {
        var userId = _userManager.GetUserId(principal);
        var user = string.IsNullOrEmpty(userId) ? null : await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }

        await _signInManager.SignOutAsync();
    }

    public bool IsAdminSignedIn(ClaimsPrincipal principal)
        => principal.Identity?.IsAuthenticated == true && principal.IsInRole(SeedData.AdminRoleName);

    public string? GetDisplayName(ClaimsPrincipal principal) => principal.Identity?.Name;

    public string? GetUserId(ClaimsPrincipal principal) => _userManager.GetUserId(principal);

    private async Task<string> BuildLockoutMessageAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        var lockoutEnd = user is null ? null : await _userManager.GetLockoutEndDateAsync(user);

        return $"Tài khoản tạm khóa do nhập sai nhiều lần. Thử lại sau khoảng {RemainingMinutes(lockoutEnd)} phút.";
    }

    private static int RemainingMinutes(DateTimeOffset? lockoutEnd)
    {
        if (lockoutEnd is null)
        {
            return 1;
        }

        var remaining = lockoutEnd.Value - DateTimeOffset.Now;

        return remaining <= TimeSpan.Zero ? 1 : Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
    }
}
