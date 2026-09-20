using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicDetailVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string ProvinceSlug { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string TypeSlug { get; set; } = string.Empty;
    public RankingLevel RankingLevel { get; set; }
    public string RankingLabel { get; set; } = string.Empty;
    public string RankingShortLabel { get; set; } = string.Empty;
    public int? RecognizedYear { get; set; }
    public int ViewCount { get; set; }

    public string Intro { get; set; } = string.Empty;
    public string LeadSentence { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionBody { get; set; } = string.Empty;
    public string? History { get; set; }

    public string? SourceUrl { get; set; }
    public string SourceHost { get; set; } = string.Empty;

    public List<RelicImageVM> Images { get; set; } = new();
    public List<TimelineEntryVM> Timeline { get; set; } = new();
    public VisitCardVM? Visit { get; set; }
    public RelicLocationMapVM? LocationMap { get; set; }

    public List<BreadcrumbItemVM> Breadcrumbs { get; set; } = new();
    public List<SectionLinkVM> SectionLinks { get; set; } = new();
    public List<RelicCardVM> RelatedSameProvince { get; set; } = new();
    public List<RelicCardVM> RelatedSameType { get; set; } = new();

    public string VisitSummary { get; set; } = string.Empty;

    public bool HasImageStrip => Images.Count > 1;

    public bool HasImages => Images.Count > 0;
    public RelicImageVM? LeadImage => Images.Count > 0 ? Images[0] : null;
    public RelicImageVM? InsetImage => Images.Count > 1 ? Images[1] : null;

    public string NameSizeModifier => Name.Length switch
    {
        > 34 => "relic-title-small",
        > 20 => "relic-title-medium",
        _ => string.Empty
    };
    public bool HasHistory => !string.IsNullOrWhiteSpace(History);
    public bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionBody);
    public bool HasTimeline => Timeline.Count > 0;
    public bool HasSource => !string.IsNullOrWhiteSpace(SourceUrl);
    public bool HasVisitCard => Visit is not null && Visit.HasAnything;
    public bool HasRelated => RelatedSameProvince.Count > 0 || RelatedSameType.Count > 0;

    public string PageTitle => Name;
    public string MetaDescription => string.IsNullOrWhiteSpace(LeadSentence)
        ? $"{Name} tại {ProvinceName}."
        : LeadSentence;
}
