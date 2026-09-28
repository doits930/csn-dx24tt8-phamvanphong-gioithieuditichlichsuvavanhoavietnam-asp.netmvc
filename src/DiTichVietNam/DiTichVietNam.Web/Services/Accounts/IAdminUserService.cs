using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Accounts;

public interface IAdminUserService
{
    Task<AdminUserListResult> GetListAsync(string? currentUserId);

    Task<AdminUserRow?> GetRowAsync(string id, string? currentUserId);

    Task<int> CountUsableAsync();

    Task<ServiceResult> CreateAsync(AdminUserCreateInput input, string currentUserId);

    Task<ServiceResult<string>> LockAsync(string id, string currentUserId);

    Task<ServiceResult<string>> UnlockAsync(string id, string currentUserId);

    Task<ServiceResult<string>> ResetPasswordAsync(string id, PasswordResetInput input, string currentUserId);

    Task<ServiceResult<string>> DeleteAsync(string id, string currentUserId);

    Task<ServiceResult<PasswordChangeStatus>> ChangeOwnPasswordAsync(string currentUserId, PasswordChangeInput input);
}
