namespace DiTichVietNam.Web.Areas.Admin.Models;

public class DashboardMissingVM
{
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CountText { get; set; } = string.Empty;
    public string ClearMessage { get; set; } = string.Empty;
    public string BarWidth { get; set; } = "0";
    public int Count { get; set; }
    public List<DashboardRelicVM> Samples { get; set; } = new();
    public bool HasAny => Count > 0;
    public int RemainingCount => Count - Samples.Count;
}
