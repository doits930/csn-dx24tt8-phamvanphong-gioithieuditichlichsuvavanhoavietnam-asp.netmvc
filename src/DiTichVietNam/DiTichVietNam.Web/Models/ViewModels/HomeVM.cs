namespace DiTichVietNam.Web.Models.ViewModels;

public class HomeVM
{
    public HomeStatsVM Stats { get; set; } = new();
    public HomeShowcaseVM Showcase { get; set; } = new();
    public List<RegionGroupVM> Regions { get; set; } = new();
    public List<RelicTypeLinkVM> RelicTypes { get; set; } = new();
}
