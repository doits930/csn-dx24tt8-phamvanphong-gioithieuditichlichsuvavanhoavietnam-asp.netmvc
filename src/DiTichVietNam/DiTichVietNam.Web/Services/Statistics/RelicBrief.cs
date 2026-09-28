namespace DiTichVietNam.Web.Services.Statistics;

public class RelicBrief
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
