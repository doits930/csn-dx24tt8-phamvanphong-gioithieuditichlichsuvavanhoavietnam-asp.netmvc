using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminUserRowVM
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Initial { get; set; } = string.Empty;

    public bool IsCurrent { get; set; }

    public AdminUserStatus Status { get; set; }

    public string StatusLabel { get; set; } = string.Empty;

    public int AccessFailedCount { get; set; }

    public string? LockBlockReason { get; set; }

    public string? UnlockBlockReason { get; set; }

    public string? DeleteBlockReason { get; set; }

    public string? ResetBlockReason { get; set; }

    public bool IsActive => Status == AdminUserStatus.Active;
}
