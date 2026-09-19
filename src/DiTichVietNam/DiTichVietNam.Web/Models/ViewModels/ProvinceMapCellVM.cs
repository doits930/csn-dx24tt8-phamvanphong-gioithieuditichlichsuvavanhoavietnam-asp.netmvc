namespace DiTichVietNam.Web.Models.ViewModels;

public class ProvinceMapCellVM
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PathData { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public int DensityStep { get; set; }
    public string LabelXPercent { get; set; } = string.Empty;
    public string LabelYPercent { get; set; } = string.Empty;

    public string CountNote => RelicCount > 0 ? $"{RelicCount} di tích" : "Chưa có di tích";
    public string AccessibleName => RelicCount > 0
        ? $"{Name}, {RelicCount} di tích"
        : $"{Name}, chưa có di tích";
}
