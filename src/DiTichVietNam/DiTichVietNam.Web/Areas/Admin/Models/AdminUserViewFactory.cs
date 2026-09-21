using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class AdminUserViewFactory
{
    public static AdminUserListVM ToListVM(AdminUserListResult result) => new()
    {
        Rows = result.Rows.Select(row => ToRowVM(row, result.UsableCount)).ToList(),
        TotalCount = result.TotalCount,
        ActiveCount = result.ActiveCount,
        TemporaryLockCount = result.TemporaryLockCount,
        LockedCount = result.LockedCount
    };

    public static AdminUserRowVM ToRowVM(AdminUserRow row, int usableCount) => new()
    {
        Id = row.Id,
        Email = row.Email,
        Initial = AdminUserFactory.Initial(row.Email),
        IsCurrent = row.IsCurrent,
        Status = row.Status,
        StatusLabel = row.StatusLabel,
        AccessFailedCount = row.AccessFailedCount,
        LockBlockReason = AdminUserFactory.LockBlockReason(row, usableCount),
        UnlockBlockReason = AdminUserFactory.UnlockBlockReason(row),
        DeleteBlockReason = AdminUserFactory.DeleteBlockReason(row, usableCount),
        ResetBlockReason = AdminUserFactory.ResetBlockReason(row)
    };

    public static AdminUserCreateInput ToInput(AdminUserFormVM vm) => new()
    {
        Email = vm.Email,
        Password = vm.Password,
        ConfirmPassword = vm.ConfirmPassword
    };

    public static PasswordResetInput ToInput(AdminUserPasswordVM vm) => new()
    {
        Password = vm.Password,
        ConfirmPassword = vm.ConfirmPassword
    };

    public static PasswordChangeInput ToInput(ChangePasswordVM vm) => new()
    {
        CurrentPassword = vm.CurrentPassword,
        NewPassword = vm.NewPassword,
        ConfirmPassword = vm.ConfirmPassword
    };
}
