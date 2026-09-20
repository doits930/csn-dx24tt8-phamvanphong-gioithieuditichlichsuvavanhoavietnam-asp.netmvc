namespace DiTichVietNam.Web.Areas.Admin.Models;

public class DashboardVM
{
    public List<DashboardStatVM> Totals { get; set; } = new();
    public List<DashboardBarVM> Rankings { get; set; } = new();
    public List<DashboardBarVM> Types { get; set; } = new();
    public List<DashboardBarVM> TopProvinces { get; set; } = new();
    public List<DashboardBarVM> Regions { get; set; } = new();
    public List<DashboardRelicVM> MostViewed { get; set; } = new();
    public List<DashboardMissingVM> Missing { get; set; } = new();
    public List<DashboardRelicVM> RecentlyChanged { get; set; } = new();
    public int RelicCount { get; set; }
    public int OtherProvinceCount { get; set; }
    public int ProvinceWithoutRelicCount { get; set; }
    public bool HasRelics => RelicCount > 0;
}
