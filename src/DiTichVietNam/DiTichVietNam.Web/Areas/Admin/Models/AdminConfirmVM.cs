namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminConfirmVM
{
    public string ElementId { get; set; } = "admin-confirm";
    public string Title { get; set; } = string.Empty;
    public string ConfirmLabel { get; set; } = string.Empty;
    public string CancelLabel { get; set; } = "Hủy";
}
