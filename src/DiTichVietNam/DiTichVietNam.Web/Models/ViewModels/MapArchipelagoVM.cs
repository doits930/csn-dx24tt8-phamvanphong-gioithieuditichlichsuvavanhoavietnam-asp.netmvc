namespace DiTichVietNam.Web.Models.ViewModels;

public class MapArchipelagoVM
{
    public string Name { get; set; } = string.Empty;
    public string ProvinceSlug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string IsletRadius { get; set; } = string.Empty;
    public List<MapIsletVM> Islets { get; set; } = new();
    public string LabelXPercent { get; set; } = string.Empty;
    public string LabelYPercent { get; set; } = string.Empty;
    public bool LabelAnchorEnd { get; set; }

    public string ProvinceNote => $"thuộc {ProvinceName}";
    public string AccessibleName => $"{Name}, {ProvinceNote}";
}

public class MapIsletVM
{
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
}
