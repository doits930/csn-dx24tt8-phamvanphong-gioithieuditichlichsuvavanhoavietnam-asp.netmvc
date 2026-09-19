using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public static class SearchBarFactory
{
    public static SearchBarVM Build(
        SearchFilterVM filter,
        List<RegionGroupVM> provinceGroups,
        List<RelicTypeLinkVM> relicTypes) => new()
    {
        Filter = filter,
        ProvinceOptionGroups = BuildProvinceOptions(provinceGroups),
        TypeOptions = relicTypes
            .Select(t => new FilterOptionVM { Value = t.Slug, Label = RelicTypeText.ShortLabel(t.Name) })
            .ToList(),
        RankingOptions = BuildRankingOptions()
    };

    private static List<FilterOptionGroupVM> BuildProvinceOptions(List<RegionGroupVM> groups) =>
        groups
            .Select(g => new FilterOptionGroupVM
            {
                DisplayName = g.DisplayName,
                Options = g.Provinces
                    .Select(p => new FilterOptionVM { Value = p.Slug, Label = p.Name })
                    .ToList()
            })
            .ToList();

    private static List<FilterOptionVM> BuildRankingOptions() =>
        new[] { RankingLevel.Provincial, RankingLevel.National, RankingLevel.SpecialNational }
            .Select(level => new FilterOptionVM
            {
                Value = ((int)level).ToString(),
                Label = RankingLevelText.ShortLabel(level)
            })
            .ToList();
}
