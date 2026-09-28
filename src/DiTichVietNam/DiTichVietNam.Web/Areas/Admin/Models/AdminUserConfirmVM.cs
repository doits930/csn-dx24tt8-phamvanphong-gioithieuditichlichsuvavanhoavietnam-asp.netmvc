namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminUserConfirmVM
{
    public string Title { get; set; } = string.Empty;

    public string Lead { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string ActionUrl { get; set; } = string.Empty;

    public string ConfirmLabel { get; set; } = string.Empty;

    public string CancelUrl { get; set; } = string.Empty;

    public bool IsDangerous { get; set; }

    public string? BlockReason { get; set; }

    public bool IsBlocked => !string.IsNullOrEmpty(BlockReason);
}
