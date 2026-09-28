using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicCardVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string TypeSlug { get; set; } = string.Empty;
    public RankingLevel RankingLevel { get; set; }
    public string RankingLabel { get; set; } = string.Empty;
    public string RankingCssModifier { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public string? MatchExcerpt { get; set; }
}
