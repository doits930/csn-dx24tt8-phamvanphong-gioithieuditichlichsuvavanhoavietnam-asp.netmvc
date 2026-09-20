namespace DiTichVietNam.Web.Services.Accounts;

public class AdminUserRow
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsCurrent { get; init; }

    public AdminUserStatus Status { get; init; }

    public string StatusLabel { get; init; } = string.Empty;

    public int AccessFailedCount { get; init; }

    public bool IsActive => Status == AdminUserStatus.Active;

    public bool IsUsable => Status != AdminUserStatus.Locked;
}
