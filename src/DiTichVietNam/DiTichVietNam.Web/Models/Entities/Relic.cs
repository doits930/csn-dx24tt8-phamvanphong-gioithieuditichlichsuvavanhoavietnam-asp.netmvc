namespace DiTichVietNam.Web.Models.Entities;

public class Relic
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string NameNoAccent { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? History { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? VisitInfo { get; set; }
    public RankingLevel RankingLevel { get; set; }
    public int? RecognizedYear { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public int ViewCount { get; set; }
    public int ProvinceId { get; set; }
    public int RelicTypeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Province? Province { get; set; }
    public RelicType? RelicType { get; set; }
    public ICollection<RelicImage> Images { get; set; } = new List<RelicImage>();
}
