using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Services.Relics;

public class RelicInput
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int ProvinceId { get; set; }
    public int RelicTypeId { get; set; }
    public RankingLevel RankingLevel { get; set; }
    public int? RecognizedYear { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? History { get; set; }
    public string? VisitInfo { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
