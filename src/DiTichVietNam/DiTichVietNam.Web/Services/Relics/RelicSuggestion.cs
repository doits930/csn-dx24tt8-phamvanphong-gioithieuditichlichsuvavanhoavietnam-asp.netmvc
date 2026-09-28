namespace DiTichVietNam.Web.Services.Relics;

public class RelicSuggestion
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
}
