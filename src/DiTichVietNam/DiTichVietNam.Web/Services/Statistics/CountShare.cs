namespace DiTichVietNam.Web.Services.Statistics;

public class CountShare
{
    public string Label { get; set; } = string.Empty;
    public string? Key { get; set; }
    public string? Modifier { get; set; }
    public int Count { get; set; }
    public int Percent { get; set; }
    public string ShareWidth { get; set; } = "0";
    public string BarWidth { get; set; } = "0";
}
