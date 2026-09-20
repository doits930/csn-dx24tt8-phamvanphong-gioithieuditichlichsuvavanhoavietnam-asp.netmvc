namespace DiTichVietNam.Web.Services.Statistics;

public class MissingDataCandidate
{
    public RelicBrief Relic { get; set; } = new();
    public bool WithoutImage { get; set; }
    public bool WithoutLocation { get; set; }
    public bool WithSingleImage { get; set; }
    public bool WithoutVisitInfo { get; set; }
}
