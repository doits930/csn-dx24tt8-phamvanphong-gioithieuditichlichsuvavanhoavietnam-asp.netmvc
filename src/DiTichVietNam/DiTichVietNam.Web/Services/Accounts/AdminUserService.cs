using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Services.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Accounts;

public class AdminUserService : IAdminUserService
{
    public const string NotFoundMessage = "Không tìm thấy tài khoản này.";
    public const string DuplicateEmailMessage = "Địa chỉ thư điện tử này đã có tài khoản.";
    public const string SaveFailedMessage = "Không lưu được tài khoản. Thử lại sau ít phút.";
    public const string WrongCurrentPasswordMessage = "Mật khẩu hiện tại không đúng.";
    public const string ChangeLockedOutMessage =
        "Nhập sai mật khẩu hiện tại quá nhiều lần nên tài khoản tạm khóa. Đăng nhập lại sau ít phút.";
    public const string ActorRejectedMessage =
        "Tài khoản của bạn không còn quyền quản trị. Đăng nhập lại để tiếp tục.";

    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly PasswordPolicy _passwordPolicy;
    private readonly AdminUserWriteLock _writeLock;

    public AdminUserService(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        PasswordPolicy passwordPolicy,
        AdminUserWriteLock writeLock)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _passwordPolicy = passwordPolicy;
        _writeLock = writeLock;
    }

    public async Task<AdminUserListResult> GetListAsync(string? currentUserId)
    {
        var now = DateTimeOffset.UtcNow;
        var rows = (await LoadUsersAsync())
            .Select(user => AdminUserFactory.ToRow(user, currentUserId, now))
            .OrderBy(row => row.Email, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new AdminUserListResult { Rows = rows };
    }

    public async Task<AdminUserRow?> GetRowAsync(string id, string? currentUserId)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(id);

        return user is null ? null : AdminUserFactory.ToRow(user, currentUserId, DateTimeOffset.UtcNow);
    }

    public async Task<int> CountUsableAsync()
        => (await LoadUsersAsync()).Count(user => !AdminUserFactory.IsIndefiniteLock(user.LockoutEnd));

    public async Task<ServiceResult> CreateAsync(AdminUserCreateInput input, string currentUserId)
    {
        var email = input.Email?.Trim() ?? string.Empty;
        var missing = _passwordPolicy.FindMissing(input.Password);
        if (missing.Count > 0)
        {
            return ServiceResult.Fail(PasswordPolicy.MissingMessage(missing), AdminUserValidator.PasswordField);
        }

        using var gate = await _writeLock.AcquireAsync();

        if (await FindActorAsync(currentUserId) is null)
        {
            return ServiceResult.Fail(ActorRejectedMessage);
        }

        if (await _userManager.FindByEmailAsync(email) is not null)
        {
            return ServiceResult.Fail(DuplicateEmailMessage, AdminUserValidator.EmailField);
        }

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var created = await _userManager.CreateAsync(user, input.Password!);
        if (!created.Succeeded)
        {
            return ToFailure(created);
        }

        var granted = await _userManager.AddToRoleAsync(user, SeedData.AdminRoleName);
        if (!granted.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return ServiceResult.Fail(SaveFailedMessage, AdminUserValidator.EmailField);
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<string>> LockAsync(string id, string currentUserId)
    {
        using var gate = await _writeLock.AcquireAsync();

        var rejected = await RejectMissingActorAsync(currentUserId);
        if (rejected is not null)
        {
            return rejected;
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult<string>.Fail(NotFoundMessage);
        }

        var row = AdminUserFactory.ToRow(user, currentUserId, DateTimeOffset.UtcNow);
        var blocked = AdminUserFactory.LockBlockReason(row, await CountUsableAsync());
        if (blocked is not null)
        {
            return ServiceResult<string>.Fail(blocked);
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        var locked = await _userManager.SetLockoutEndDateAsync(user, AdminUserFactory.IndefiniteLockEnd);
        if (!locked.Succeeded)
        {
            return ServiceResult<string>.Fail(SaveFailedMessage);
        }

        await _userManager.UpdateSecurityStampAsync(user);

        return ServiceResult<string>.Ok(row.Email);
    }

    public async Task<ServiceResult<string>> UnlockAsync(string id, string currentUserId)
    {
        using var gate = await _writeLock.AcquireAsync();

        var rejected = await RejectMissingActorAsync(currentUserId);
        if (rejected is not null)
        {
            return rejected;
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult<string>.Fail(NotFoundMessage);
        }

        var row = AdminUserFactory.ToRow(user, currentUserId, DateTimeOffset.UtcNow);
        var blocked = AdminUserFactory.UnlockBlockReason(row);
        if (blocked is not null)
        {
            return ServiceResult<string>.Fail(blocked);
        }

        var unlocked = await _userManager.SetLockoutEndDateAsync(user, null);
        if (!unlocked.Succeeded)
        {
            return ServiceResult<string>.Fail(SaveFailedMessage);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return ServiceResult<string>.Ok(row.Email);
    }

    public async Task<ServiceResult<string>> ResetPasswordAsync(
        string id,
        PasswordResetInput input,
        string currentUserId)
    {
        var missing = _passwordPolicy.FindMissing(input.Password);
        if (missing.Count > 0)
        {
            return ServiceResult<string>.Fail(
                PasswordPolicy.MissingMessage(missing),
                AdminUserValidator.PasswordField);
        }

        using var gate = await _writeLock.AcquireAsync();

        var rejected = await RejectMissingActorAsync(currentUserId);
        if (rejected is not null)
        {
            return rejected;
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult<string>.Fail(NotFoundMessage);
        }

        var row = AdminUserFactory.ToRow(user, currentUserId, DateTimeOffset.UtcNow);
        var blocked = AdminUserFactory.ResetBlockReason(row);
        if (blocked is not null)
        {
            return ServiceResult<string>.Fail(blocked);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var reset = await _userManager.ResetPasswordAsync(user, token, input.Password!);
        if (!reset.Succeeded)
        {
            var failure = ToFailure(reset);

            return ServiceResult<string>.Fail(failure.Error ?? SaveFailedMessage, failure.Field);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        return ServiceResult<string>.Ok(row.Email);
    }

    public async Task<ServiceResult<string>> DeleteAsync(string id, string currentUserId)
    {
        using var gate = await _writeLock.AcquireAsync();

        var rejected = await RejectMissingActorAsync(currentUserId);
        if (rejected is not null)
        {
            return rejected;
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return ServiceResult<string>.Fail(NotFoundMessage);
        }

        var row = AdminUserFactory.ToRow(user, currentUserId, DateTimeOffset.UtcNow);
        var blocked = AdminUserFactory.DeleteBlockReason(row, await CountUsableAsync());
        if (blocked is not null)
        {
            return ServiceResult<string>.Fail(blocked);
        }

        var removed = await _userManager.DeleteAsync(user);

        return removed.Succeeded
            ? ServiceResult<string>.Ok(row.Email)
            : ServiceResult<string>.Fail(SaveFailedMessage);
    }

    public async Task<ServiceResult<PasswordChangeStatus>> ChangeOwnPasswordAsync(
        string currentUserId,
        PasswordChangeInput input)
    {
        var user = await FindActorAsync(currentUserId);
        if (user is null)
        {
            return ServiceResult<PasswordChangeStatus>.Fail(ActorRejectedMessage, PasswordChangeStatus.LockedOut);
        }

        var missing = _passwordPolicy.FindMissing(input.NewPassword);
        if (missing.Count > 0)
        {
            return ServiceResult<PasswordChangeStatus>.Fail(
                PasswordPolicy.MissingMessage(missing),
                PasswordChangeStatus.Rejected,
                AdminUserValidator.NewPasswordField);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _signInManager.SignOutAsync();

            return ServiceResult<PasswordChangeStatus>.Fail(ChangeLockedOutMessage, PasswordChangeStatus.LockedOut);
        }

        if (!await _userManager.CheckPasswordAsync(user, input.CurrentPassword ?? string.Empty))
        {
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
            {
                await _signInManager.SignOutAsync();

                return ServiceResult<PasswordChangeStatus>.Fail(ChangeLockedOutMessage, PasswordChangeStatus.LockedOut);
            }

            return ServiceResult<PasswordChangeStatus>.Fail(
                WrongCurrentPasswordMessage,
                PasswordChangeStatus.WrongCurrentPassword,
                AdminUserValidator.CurrentPasswordField);
        }

        var changed = await _userManager.ChangePasswordAsync(user, input.CurrentPassword!, input.NewPassword!);
        if (!changed.Succeeded)
        {
            var failure = ToFailure(changed);

            return ServiceResult<PasswordChangeStatus>.Fail(
                failure.Error ?? SaveFailedMessage,
                PasswordChangeStatus.Rejected,
                failure.Field);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _signInManager.RefreshSignInAsync(user);

        return ServiceResult<PasswordChangeStatus>.Ok(PasswordChangeStatus.Changed);
    }

    private async Task<IdentityUser?> FindActorAsync(string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return null;
        }

        var actor = await _userManager.FindByIdAsync(currentUserId);

        return actor is null || AdminUserFactory.IsIndefiniteLock(actor.LockoutEnd) ? null : actor;
    }

    private async Task<ServiceResult<string>?> RejectMissingActorAsync(string currentUserId)
        => await FindActorAsync(currentUserId) is null
            ? ServiceResult<string>.Fail(ActorRejectedMessage)
            : null;

    private async Task<List<IdentityUser>> LoadUsersAsync()
        => await _userManager.Users.AsNoTracking().ToListAsync();

    private static ServiceResult ToFailure(IdentityResult result)
    {
        var error = result.Errors.FirstOrDefault();
        if (error is null)
        {
            return ServiceResult.Fail(SaveFailedMessage);
        }

        var (field, message) = AdminUserValidator.Translate(error);

        return ServiceResult.Fail(message, field.Length == 0 ? null : field);
    }
}
