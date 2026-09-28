namespace DiTichVietNam.Web.Services.Statistics;

public class DashboardStatistics
{
    public int RelicCount { get; set; }
    public int ImageCount { get; set; }
    public int ViewCount { get; set; }
    public int ProvinceCount { get; set; }
    public int ProvinceWithRelicCount { get; set; }
    public List<CountShare> Rankings { get; set; } = new();
    public List<CountShare> Types { get; set; } = new();
    public List<CountShare> TopProvinces { get; set; } = new();
    public int OtherProvinceCount { get; set; }
    public List<CountShare> Regions { get; set; } = new();
    public List<RelicBrief> MostViewed { get; set; } = new();
    public List<MissingDataGroup> Missing { get; set; } = new();
    public List<RelicBrief> RecentlyChanged { get; set; } = new();
}
