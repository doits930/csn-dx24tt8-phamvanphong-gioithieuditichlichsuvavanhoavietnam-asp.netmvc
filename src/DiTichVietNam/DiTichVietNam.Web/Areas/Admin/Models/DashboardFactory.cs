using System.Globalization;
using DiTichVietNam.Web.Areas.Admin.Controllers;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Statistics;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class DashboardFactory
{
    private static readonly CultureInfo Display = CultureInfo.GetCultureInfo("vi-VN");

    public static DashboardVM ToVM(DashboardStatistics data)
    {
        return new DashboardVM
        {
            RelicCount = data.RelicCount,
            OtherProvinceCount = data.OtherProvinceCount,
            ProvinceWithoutRelicCount = data.ProvinceCount - data.ProvinceWithRelicCount,
            Totals = BuildTotals(data),
            Rankings = data.Rankings.Select(item => ToBar(item, SearchFilterVM.RankingKey)).ToList(),
            Types = data.Types.Select(item => ToBar(item, SearchFilterVM.TypeKey, TypeUnit(item))).ToList(),
            TopProvinces = data.TopProvinces.Select(item => ToBar(item, SearchFilterVM.ProvinceKey)).ToList(),
            Regions = data.Regions.Select(item => ToBar(item, null)).ToList(),
            MostViewed = data.MostViewed.Select(ToRelic).ToList(),
            RecentlyChanged = data.RecentlyChanged.Select(ToRelic).ToList(),
            Missing = data.Missing.Select(group => ToMissing(group, data.RelicCount)).ToList()
        };
    }

    private static List<DashboardStatVM> BuildTotals(DashboardStatistics data) => new()
    {
        new DashboardStatVM
        {
            Label = "Mục trong hệ thống",
            Value = Number(data.RelicCount),
            Icon = "relic",
            IconClass = "admin-stat-icon"
        },
        new DashboardStatVM
        {
            Label = "Ảnh đã tải lên",
            Value = Number(data.ImageCount),
            Icon = "image",
            IconClass = "admin-stat-icon admin-stat-icon-stone"
        },
        new DashboardStatVM
        {
            Label = "Lượt xem trang chi tiết",
            Value = Number(data.ViewCount),
            Icon = "views",
            IconClass = "admin-stat-icon admin-stat-icon-gilt"
        },
        new DashboardStatVM
        {
            Label = "Tỉnh thành đã có dữ liệu",
            Value = $"{Number(data.ProvinceWithRelicCount)}/{Number(data.ProvinceCount)}",
            Icon = "province",
            IconClass = "admin-stat-icon admin-stat-icon-vermilion"
        }
    };

    private const string MixedUnit = "mục";

    private static string TypeUnit(CountShare item) =>
        IntangibleHeritage.IsIntangible(item.Key, item.Label) ? MixedUnit : "di tích";

    private static DashboardBarVM ToBar(CountShare item, string? filterKey, string unit = MixedUnit) => new()
    {
        Label = item.Label,
        Unit = unit,
        Count = item.Count,
        CountText = Number(item.Count),
        PercentText = $"{item.Percent}%",
        ShareWidth = item.ShareWidth,
        BarWidth = item.BarWidth,
        Modifier = filterKey == SearchFilterVM.RankingKey ? item.Modifier : null,
        Url = BuildFilterUrl(filterKey, item.Key, item.Count)
    };

    private static string? BuildFilterUrl(string? filterKey, string? value, int count)
    {
        if (filterKey is null || string.IsNullOrWhiteSpace(value) || count <= 0)
        {
            return null;
        }

        return $"{RelicAdminController.ListPath}?{filterKey}={Uri.EscapeDataString(value)}";
    }

    private static DashboardRelicVM ToRelic(RelicBrief relic) => new()
    {
        Name = relic.Name,
        Slug = relic.Slug,
        ProvinceName = relic.ProvinceName,
        ThumbnailPath = relic.ThumbnailPath,
        EditUrl = $"{RelicAdminController.ListPath}/sua/{relic.Id}",
        ViewCountText = Number(relic.ViewCount),
        ChangedAtText = (relic.UpdatedAt ?? relic.CreatedAt).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
        ChangedNote = relic.UpdatedAt.HasValue ? "đã sửa" : null
    };

    private static DashboardMissingVM ToMissing(MissingDataGroup group, int relicCount) => new()
    {
        Label = group.Label,
        Icon = group.Icon,
        Count = group.Count,
        CountText = Number(group.Count),
        ClearMessage = group.ClearMessage,
        Note = group.Note,
        BarWidth = StatisticsFactory.Width(group.Count, relicCount),
        Samples = group.Samples.Select(ToRelic).ToList()
    };

    private static string Number(int value) => value.ToString("N0", Display);
}
