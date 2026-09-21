namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminBlockedButtonVM
{
    public string NoteId { get; set; } = string.Empty;

    public string Icon { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public bool ShowLabel { get; set; }
}
