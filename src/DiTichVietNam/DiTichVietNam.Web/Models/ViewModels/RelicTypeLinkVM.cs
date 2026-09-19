namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicTypeLinkVM
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RelicCount { get; set; }
    public string? CoverImagePath { get; set; }
}
