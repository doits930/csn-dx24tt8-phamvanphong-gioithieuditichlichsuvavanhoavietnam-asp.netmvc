using System.Globalization;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Provinces;

public static class ProvinceFactory
{
    public const int MaxDensityStep = 4;

    public static ProvinceLinkVM ToLinkVM(Province province, int relicCount) => new()
    {
        Name = province.Name,
        Slug = province.Slug,
        RelicCount = relicCount
    };

    public static ProvinceAdminRow ToAdminRow(Province province, int relicCount) => new()
    {
        Id = province.Id,
        Name = province.Name,
        NameNoAccent = SlugHelper.RemoveDiacritics(province.Name),
        Slug = province.Slug,
        Region = province.Region,
        RegionLabel = RegionText.DisplayName(province.Region),
        RelicCount = relicCount,
        HasMapShape = VietnamMapGeometry.HasProvinceShape(province.Slug)
    };

    public static ProvinceEditData ToEditData(Province province, int relicCount) => new()
    {
        Id = province.Id,
        Name = province.Name,
        Slug = province.Slug,
        Region = province.Region,
        RelicCount = relicCount,
        HasMapShape = VietnamMapGeometry.HasProvinceShape(province.Slug)
    };

    public static Province ToEntity(ProvinceInput input, string slug) => new()
    {
        Name = input.Name?.Trim() ?? string.Empty,
        Slug = slug,
        Region = input.Region?.Trim() ?? string.Empty
    };

    public static string? DeleteBlockReason(int relicCount, bool hasMapShape)
    {
        if (relicCount > 0)
        {
            return $"Không xóa được tỉnh thành này vì còn {relicCount} di tích đang thuộc về nó. "
                + "Chuyển những di tích đó sang tỉnh thành khác rồi xóa lại.";
        }

        if (hasMapShape)
        {
            return "Không xóa được tỉnh thành này vì nó thuộc danh mục 34 tỉnh thành hiện hành "
                + "và đang là một mảng trên bản đồ trang chủ. Đổi tên thì được, xóa thì bản đồ mất một mảng.";
        }

        return null;
    }

    public static int DensityStep(int relicCount) =>
        relicCount <= 0 ? 0 : Math.Min(relicCount, MaxDensityStep);

    public static VietnamMapVM ToMapVM(IReadOnlyDictionary<string, ProvinceLinkVM> provincesBySlug) => new()
    {
        ViewBox = VietnamMapGeometry.ViewBox,
        Provinces = VietnamMapGeometry.Provinces
            .Select(shape => ToMapCellVM(shape, provincesBySlug))
            .ToList(),
        Archipelagos = VietnamMapGeometry.Archipelagos
            .Select(shape => ToArchipelagoVM(shape, provincesBySlug))
            .ToList(),
        Islands = VietnamMapGeometry.Islands
            .Select(shape => ToIslandVM(shape, provincesBySlug))
            .ToList(),
        LegendSteps = BuildLegendSteps()
    };

    private static ProvinceMapCellVM ToMapCellVM(
        ProvinceShape shape,
        IReadOnlyDictionary<string, ProvinceLinkVM> provincesBySlug)
    {
        provincesBySlug.TryGetValue(shape.Slug, out var province);
        var relicCount = province?.RelicCount ?? 0;

        return new ProvinceMapCellVM
        {
            Slug = shape.Slug,
            Name = province?.Name ?? shape.Name,
            PathData = shape.PathData,
            RelicCount = relicCount,
            DensityStep = DensityStep(relicCount),
            LabelXPercent = ToXPercent(shape.LabelX),
            LabelYPercent = ToYPercent(shape.LabelY)
        };
    }

    private static MapArchipelagoVM ToArchipelagoVM(
        ArchipelagoShape shape,
        IReadOnlyDictionary<string, ProvinceLinkVM> provincesBySlug)
    {
        provincesBySlug.TryGetValue(shape.ProvinceSlug, out var province);

        return new MapArchipelagoVM
        {
            Name = shape.Name,
            ProvinceSlug = shape.ProvinceSlug,
            ProvinceName = province?.Name ?? shape.ProvinceSlug,
            IsletRadius = Format(VietnamMapGeometry.IsletRadius),
            Islets = shape.Islets
                .Select(p => new MapIsletVM { X = Format(p.X), Y = Format(p.Y) })
                .ToList(),
            LabelXPercent = ToXPercent(shape.LabelX),
            LabelYPercent = ToYPercent(shape.LabelY),
            LabelAnchorEnd = shape.LabelAnchorEnd
        };
    }

    private static MapIslandVM ToIslandVM(
        IslandShape shape,
        IReadOnlyDictionary<string, ProvinceLinkVM> provincesBySlug)
    {
        provincesBySlug.TryGetValue(shape.ProvinceSlug, out var province);

        return new MapIslandVM
        {
            Name = shape.Name,
            ProvinceSlug = shape.ProvinceSlug,
            ProvinceName = province?.Name ?? shape.ProvinceSlug,
            X = Format(shape.X),
            Y = Format(shape.Y),
            Radius = Format(VietnamMapGeometry.IslandRadius),
            ShowMarker = shape.ShowMarker,
            ShowLabel = shape.ShowLabel,
            LabelText = shape.LabelText,
            LabelXPercent = ToXPercent(shape.X),
            LabelYPercent = ToYPercent(shape.Y + shape.LabelOffsetY)
        };
    }

    private static List<MapLegendStepVM> BuildLegendSteps()
    {
        var steps = new List<MapLegendStepVM>();

        for (var step = 0; step < MaxDensityStep; step++)
        {
            steps.Add(new MapLegendStepVM { Step = step, Label = step.ToString() });
        }

        steps.Add(new MapLegendStepVM { Step = MaxDensityStep, Label = $"{MaxDensityStep}+" });
        return steps;
    }

    private static string ToXPercent(double x) =>
        Format(Math.Round(x / VietnamMapGeometry.ViewBoxWidth * 100, 2));

    private static string ToYPercent(double y) =>
        Format(Math.Round(y / VietnamMapGeometry.ViewBoxHeight * 100, 2));

    private static string Format(double value) =>
        value.ToString(CultureInfo.InvariantCulture);
}
