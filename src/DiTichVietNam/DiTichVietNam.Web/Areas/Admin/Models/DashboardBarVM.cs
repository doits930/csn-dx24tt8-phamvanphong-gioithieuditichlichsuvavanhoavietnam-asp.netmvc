namespace DiTichVietNam.Web.Areas.Admin.Models;

public class DashboardBarVM
{
    public string Label { get; set; } = string.Empty;
    public string CountText { get; set; } = string.Empty;
    public string PercentText { get; set; } = string.Empty;
    public string ShareWidth { get; set; } = "0";
    public string BarWidth { get; set; } = "0";
    public string? Url { get; set; }
    public string? Modifier { get; set; }
    public int Count { get; set; }
    public bool HasAny => Count > 0;
}
