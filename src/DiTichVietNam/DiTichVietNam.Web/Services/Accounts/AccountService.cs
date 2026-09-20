using System.Security.Claims;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;
using Microsoft.AspNetCore.Identity;

namespace DiTichVietNam.Web.Services.Accounts;

public class AccountService : IAccountService
{
    public const string GenericFailureMessage = "Tài khoản hoặc mật khẩu không đúng.";

    private const string DecoyPassword = "decoy-password-for-constant-time";

    private static string? _decoyHash;

    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public async Task<ServiceResult<LoginStatus>> SignInAsync(LoginVM vm)
    {
        var userName = vm.UserName?.Trim();
        if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(vm.Password))
        {
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            SpendHashingTime(vm.Password);
            return ServiceResult<LoginStatus>.Fail(GenericFailureMessage, LoginStatus.InvalidCredentials);
        }

        var attempt = await _signInManager.PasswordSignInAsync(user, vm.Password, isPersistent: false, lockoutOnFailure: true);

        if (attempt.IsLockedOut)
        {
            return ServiceResult<LoginStatus>.Fail(await BuildLockoutMessageAsync(userName), LoginStatus.LockedOut);
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

    public Task SignOutAsync() => _signInManager.SignOutAsync();

    public bool IsAdminSignedIn(ClaimsPrincipal principal)
        => principal.Identity?.IsAuthenticated == true && principal.IsInRole(SeedData.AdminRoleName);

    public string? GetDisplayName(ClaimsPrincipal principal) => principal.Identity?.Name;

    private async Task<string> BuildLockoutMessageAsync(string userName)
    {
        var minutes = await GetRemainingLockoutMinutesAsync(userName);
        return $"Tài khoản tạm khóa do nhập sai nhiều lần. Thử lại sau khoảng {minutes} phút.";
    }

    private async Task<int> GetRemainingLockoutMinutesAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user is null)
        {
            return 1;
        }

        var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
        if (lockoutEnd is null)
        {
            return 1;
        }

        var remaining = lockoutEnd.Value - DateTimeOffset.Now;
        if (remaining <= TimeSpan.Zero)
        {
            return 1;
        }

        return Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
    }
}
