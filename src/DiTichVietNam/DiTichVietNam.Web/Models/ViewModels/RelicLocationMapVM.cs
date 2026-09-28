namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicLocationMapVM
{
    public string ViewBox { get; set; } = string.Empty;
    public List<LocationMapProvinceVM> Provinces { get; set; } = new();
    public List<LocationMapArchipelagoVM> Archipelagos { get; set; } = new();
    public List<LocationMapIsletVM> Islands { get; set; } = new();
    public string? MarkerX { get; set; }
    public string? MarkerY { get; set; }
    public string MarkerRadius { get; set; } = string.Empty;
    public string MarkerHaloRadius { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public string Credit { get; set; } = string.Empty;

    public bool HasMarker => MarkerX is not null && MarkerY is not null;
}

public class LocationMapProvinceVM
{
    public string PathData { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
}

public class LocationMapArchipelagoVM
{
    public string Name { get; set; } = string.Empty;
    public string LabelText { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string LabelXPercent { get; set; } = string.Empty;
    public string LabelYPercent { get; set; } = string.Empty;
    public bool LabelAnchorEnd { get; set; }
    public string IsletRadius { get; set; } = string.Empty;
    public List<LocationMapIsletVM> Islets { get; set; } = new();
}

public class LocationMapIsletVM
{
    public string Title { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
    public string Radius { get; set; } = string.Empty;
}
