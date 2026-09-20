namespace DiTichVietNam.Web.Areas.Admin.Models;

public class DashboardRelicVM
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public string EditUrl { get; set; } = string.Empty;
    public string ViewCountText { get; set; } = string.Empty;
    public string ChangedAtText { get; set; } = string.Empty;
    public string? ChangedNote { get; set; }
}
