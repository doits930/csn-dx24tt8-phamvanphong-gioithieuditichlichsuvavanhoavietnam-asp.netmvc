namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicSuggestionVM
{
    public string Name { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string EditUrl { get; set; } = string.Empty;
}
