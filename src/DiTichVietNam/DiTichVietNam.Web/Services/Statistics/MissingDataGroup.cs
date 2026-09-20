namespace DiTichVietNam.Web.Services.Statistics;

public class MissingDataGroup
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ClearMessage { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<RelicBrief> Samples { get; set; } = new();
}
