namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminUserListVM
{
    public List<AdminUserRowVM> Rows { get; set; } = new();

    public int TotalCount { get; set; }

    public int ActiveCount { get; set; }

    public int TemporaryLockCount { get; set; }

    public int LockedCount { get; set; }

    public string ResultSummary => $"Đang có {TotalCount} tài khoản quản trị.";
}
