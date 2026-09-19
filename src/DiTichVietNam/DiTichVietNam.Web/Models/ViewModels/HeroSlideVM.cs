using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Models.ViewModels;

public class HeroSlideVM
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public RankingLevel RankingLevel { get; set; }
    public string RankingLabel { get; set; } = string.Empty;
    public int? RecognizedYear { get; set; }
    public string Intro { get; set; } = string.Empty;
    public string MainImagePath { get; set; } = string.Empty;
    public string? MainImageCaption { get; set; }
    public string? InsetImagePath { get; set; }
    public string? InsetImageCaption { get; set; }

    public bool HasInsetImage => !string.IsNullOrWhiteSpace(InsetImagePath);

    public string NameSizeModifier => Name.Length switch
    {
        <= 20 => string.Empty,
        <= 34 => "lookbook-name-medium",
        _ => "lookbook-name-small"
    };
}
