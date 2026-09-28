using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Services.Relics;

public class RelicAdminRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public RankingLevel RankingLevel { get; set; }
    public string RankingShortLabel { get; set; } = string.Empty;
    public string RankingCssModifier { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public int ImageCount { get; set; }
    public int ViewCount { get; set; }
    public DateTime ChangedAt { get; set; }
    public bool WasEdited { get; set; }
}
