namespace DiTichVietNam.Web.Models.ViewModels;

public class MapIslandVM
{
    public string Name { get; set; } = string.Empty;
    public string ProvinceSlug { get; set; } = string.Empty;
    public string ProvinceName { get; set; } = string.Empty;
    public string X { get; set; } = string.Empty;
    public string Y { get; set; } = string.Empty;
    public string Radius { get; set; } = string.Empty;
    public bool ShowMarker { get; set; }
    public bool ShowLabel { get; set; }
    public string LabelText { get; set; } = string.Empty;
    public string LabelXPercent { get; set; } = string.Empty;
    public string LabelYPercent { get; set; } = string.Empty;

    public string ProvinceNote => $"thuộc {ProvinceName}";
    public string Title => $"{Name} ({ProvinceName})";
}
