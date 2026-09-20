using DiTichVietNam.Web.Services.Relics;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicFormVM
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string? ProvinceId { get; set; }

    public string? RelicTypeId { get; set; }

    public string? RankingLevel { get; set; }

    public string? RecognizedYear { get; set; }

    public string? Description { get; set; }

    public string? History { get; set; }

    public string? VisitInfo { get; set; }

    public string? SourceUrl { get; set; }

    public string? Latitude { get; set; }

    public string? Longitude { get; set; }

    public string? ReturnUrl { get; set; }

    public string Slug { get; set; } = string.Empty;

    public int ImageCount { get; set; }

    public RelicFormOptions Options { get; set; } = new();

    public bool IsEdit => Id > 0;
}
