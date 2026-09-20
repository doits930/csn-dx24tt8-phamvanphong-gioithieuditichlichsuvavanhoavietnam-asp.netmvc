using Microsoft.AspNetCore.Identity;

namespace DiTichVietNam.Web.Services.Accounts;

public static class AdminUserFactory
{
    public const string SelfLockReason = "Không khóa được tài khoản bạn đang đăng nhập.";
    public const string SelfUnlockReason = "Không mở khóa được tài khoản bạn đang đăng nhập.";
    public const string SelfDeleteReason = "Không xóa được tài khoản bạn đang đăng nhập.";
    public const string SelfResetReason = "Mật khẩu của tài khoản đang đăng nhập đổi ở trang Đổi mật khẩu.";
    public const string LastUsableLockReason = "Đây là tài khoản dùng được cuối cùng nên không khóa được.";
    public const string LastUsableDeleteReason = "Đây là tài khoản dùng được cuối cùng nên không xóa được.";

    public const int IndefiniteLockYear = 9000;

    public static readonly DateTimeOffset IndefiniteLockEnd =
        new(9999, 12, 31, 23, 59, 59, TimeSpan.Zero);

    public static bool IsIndefiniteLock(DateTimeOffset? lockoutEnd)
        => lockoutEnd is not null && lockoutEnd.Value.Year >= IndefiniteLockYear;

    public static AdminUserRow ToRow(IdentityUser user, string? currentUserId, DateTimeOffset now)
    {
        var status = ToStatus(user.LockoutEnd, now);

        return new AdminUserRow
        {
            Id = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            IsCurrent = currentUserId is not null && user.Id == currentUserId,
            Status = status,
            StatusLabel = ToStatusLabel(status, user.LockoutEnd),
            AccessFailedCount = user.AccessFailedCount
        };
    }

    public static string Initial(string email)
        => string.IsNullOrEmpty(email) ? "Q" : email[..1].ToUpperInvariant();

    public static string? LockBlockReason(AdminUserRow row, int usableCount)
    {
        if (row.IsCurrent)
        {
            return SelfLockReason;
        }

        return row.IsUsable && usableCount <= 1 ? LastUsableLockReason : null;
    }

    public static string? UnlockBlockReason(AdminUserRow row)
        => row.IsCurrent ? SelfUnlockReason : null;

    public static string? DeleteBlockReason(AdminUserRow row, int usableCount)
    {
        if (row.IsCurrent)
        {
            return SelfDeleteReason;
        }

        return row.IsUsable && usableCount <= 1 ? LastUsableDeleteReason : null;
    }

    public static string? ResetBlockReason(AdminUserRow row)
        => row.IsCurrent ? SelfResetReason : null;

    private static AdminUserStatus ToStatus(DateTimeOffset? lockoutEnd, DateTimeOffset now)
    {
        if (IsIndefiniteLock(lockoutEnd))
        {
            return AdminUserStatus.Locked;
        }

        return lockoutEnd is null || lockoutEnd <= now
            ? AdminUserStatus.Active
            : AdminUserStatus.TemporaryLock;
    }

    private static string ToStatusLabel(AdminUserStatus status, DateTimeOffset? lockoutEnd) => status switch
    {
        AdminUserStatus.Locked => "Đang khóa",
        AdminUserStatus.TemporaryLock => $"Tạm khóa đến {lockoutEnd!.Value.ToLocalTime():HH:mm dd/MM}",
        _ => "Đang hoạt động"
    };
}
