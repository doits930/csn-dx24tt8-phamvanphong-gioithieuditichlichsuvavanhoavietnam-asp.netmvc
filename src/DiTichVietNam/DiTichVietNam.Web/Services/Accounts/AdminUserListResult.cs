namespace DiTichVietNam.Web.Services.Accounts;

public class AdminUserListResult
{
    public List<AdminUserRow> Rows { get; init; } = new();

    public int TotalCount => Rows.Count;

    public int ActiveCount => Rows.Count(row => row.Status == AdminUserStatus.Active);

    public int TemporaryLockCount => Rows.Count(row => row.Status == AdminUserStatus.TemporaryLock);

    public int LockedCount => Rows.Count(row => row.Status == AdminUserStatus.Locked);

    public int UsableCount => Rows.Count(row => row.IsUsable);
}
