namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicListVM
{
    public List<RelicCardVM> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public SearchFilterVM Filter { get; set; } = new();
    public PaginationVM Pagination { get; set; } = new();
    public SearchBarVM SearchBar { get; set; } = new();

    public string PageTitle { get; set; } = string.Empty;
    public string? HeaderNote { get; set; }
    public string FilterSummary { get; set; } = string.Empty;
    public string BasePath { get; set; } = RelicListPaths.AllRelics;
    public string ClearFilterUrl { get; set; } = RelicListPaths.AllRelics;

    public List<BreadcrumbItemVM> Breadcrumbs { get; set; } = new();
    public string? SuggestionTitle { get; set; }
    public List<SuggestionLinkVM> SuggestionLinks { get; set; } = new();

    public bool HasResults => Items.Count > 0;
    public bool ShowsMatchExcerpt => Items.Any(i => !string.IsNullOrWhiteSpace(i.MatchExcerpt));
    public bool ShowsSuggestions => SuggestionLinks.Count > 0 && !string.IsNullOrWhiteSpace(SuggestionTitle);
}
