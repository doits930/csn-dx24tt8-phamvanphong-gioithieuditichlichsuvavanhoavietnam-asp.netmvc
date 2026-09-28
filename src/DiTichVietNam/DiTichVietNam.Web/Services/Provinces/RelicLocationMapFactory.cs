using System.Globalization;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Provinces;

public static class RelicLocationMapFactory
{
    private const double MarkerRadius = 13;
    private const double MarkerHaloRadius = 30;
    private const double LabelClearance = 26;

    private const string CreditLine =
        "Sơ đồ minh họa vị trí. Ranh giới từ geoBoundaries gbOpen VNM ADM1 (Runfola và cộng sự, 2020; CC BY 4.0); không dùng để xác định địa giới.";

    public static RelicLocationMapVM Build(
        string provinceSlug,
        string provinceName,
        string regionName,
        double? latitude,
        double? longitude)
    {
        var model = new RelicLocationMapVM
        {
            ViewBox = VietnamMapGeometry.ViewBox,
            MarkerRadius = Format(MarkerRadius),
            MarkerHaloRadius = Format(MarkerHaloRadius),
            Caption = BuildCaption(provinceName, regionName),
            Credit = CreditLine,
            Provinces = VietnamMapGeometry.Provinces
                .Select(shape => new LocationMapProvinceVM
                {
                    PathData = shape.PathData,
                    IsCurrent = shape.Slug == provinceSlug
                })
                .ToList(),
            Archipelagos = VietnamMapGeometry.Archipelagos
                .Select(shape => new LocationMapArchipelagoVM
                {
                    Name = shape.Name,
                    LabelText = ShortLabel(shape.Name),
                    ProvinceName = ProvinceNameOf(shape.ProvinceSlug),
                    LabelXPercent = ToXPercent(LabelAnchorX(shape)),
                    LabelYPercent = ToYPercent(shape.Islets.Max(point => point.Y) + LabelClearance),
                    LabelAnchorEnd = shape.LabelAnchorEnd,
                    IsletRadius = Format(VietnamMapGeometry.IsletRadius),
                    Islets = shape.Islets
                        .Select(point => new LocationMapIsletVM
                        {
                            Title = shape.Name,
                            X = Format(point.X),
                            Y = Format(point.Y),
                            Radius = Format(VietnamMapGeometry.IsletRadius)
                        })
                        .ToList()
                })
                .ToList(),
            Islands = VietnamMapGeometry.Islands
                .Where(shape => shape.ShowMarker)
                .Select(shape => new LocationMapIsletVM
                {
                    Title = shape.Name,
                    X = Format(shape.X),
                    Y = Format(shape.Y),
                    Radius = Format(VietnamMapGeometry.IslandRadius)
                })
                .ToList()
        };

        if (MapProjection.TryProject(latitude, longitude, out var x, out var y))
        {
            model.MarkerX = Format(x);
            model.MarkerY = Format(y);
        }

        return model;
    }

    private static string BuildCaption(string provinceName, string regionName)
    {
        if (string.IsNullOrWhiteSpace(provinceName))
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(regionName)
            ? $"Thuộc {provinceName}."
            : $"Thuộc {provinceName}, {regionName}.";
    }

    private static double LabelAnchorX(ArchipelagoShape shape) => shape.LabelAnchorEnd
        ? shape.Islets.Max(point => point.X) + VietnamMapGeometry.IsletRadius
        : shape.Islets.Average(point => point.X);

    private static string ProvinceNameOf(string slug) =>
        VietnamMapGeometry.Provinces.FirstOrDefault(shape => shape.Slug == slug)?.Name ?? string.Empty;

    private static string ShortLabel(string name) =>
        name.StartsWith("Quần đảo ", StringComparison.Ordinal) ? name["Quần đảo ".Length..] : name;

    private static string ToXPercent(double value) =>
        Format(Math.Round(value / VietnamMapGeometry.ViewBoxWidth * 100, 2));

    private static string ToYPercent(double value) =>
        Format(Math.Round(value / VietnamMapGeometry.ViewBoxHeight * 100, 2));

    private static string Format(double value) => value.ToString(CultureInfo.InvariantCulture);
}
