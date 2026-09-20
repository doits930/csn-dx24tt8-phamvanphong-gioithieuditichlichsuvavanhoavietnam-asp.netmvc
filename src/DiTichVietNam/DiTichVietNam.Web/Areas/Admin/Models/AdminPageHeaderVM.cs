namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminPageHeaderVM
{
    public string Title { get; set; } = string.Empty;
    public string? Lead { get; set; }
    public string? ActionLabel { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionIcon { get; set; }
}
