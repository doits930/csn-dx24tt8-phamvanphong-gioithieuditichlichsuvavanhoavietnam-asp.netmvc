namespace DiTichVietNam.Web.Models.ViewModels;

public class SearchBarVM
{
    public SearchFilterVM Filter { get; set; } = new();
    public List<FilterOptionGroupVM> ProvinceOptionGroups { get; set; } = new();
    public List<FilterOptionVM> TypeOptions { get; set; } = new();
    public List<FilterOptionVM> RankingOptions { get; set; } = new();
    public const string AllOptionLabel = "Tất cả";

    public bool HasSelectedFilter =>
        !string.IsNullOrWhiteSpace(Filter.ProvinceSlug)
        || !string.IsNullOrWhiteSpace(Filter.TypeSlug)
        || Filter.Ranking.HasValue;

    public string SelectedProvinceLabel => ProvinceOptionGroups
        .SelectMany(g => g.Options)
        .FirstOrDefault(o => o.Value == Filter.ProvinceSlug)?.Label ?? AllOptionLabel;

    public string SelectedTypeLabel => TypeOptions
        .FirstOrDefault(o => o.Value == Filter.TypeSlug)?.Label ?? AllOptionLabel;

    public string SelectedRankingLabel => Filter.Ranking.HasValue
        ? RankingOptions.FirstOrDefault(o => o.Value == ((int)Filter.Ranking.Value).ToString())?.Label ?? AllOptionLabel
        : AllOptionLabel;
}
