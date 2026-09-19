namespace DiTichVietNam.Web.Models.ViewModels;

public class HomeVM
{
    public HomeShowcaseVM Showcase { get; set; } = new();
    public SearchBarVM SearchBar { get; set; } = new();
    public List<RegionGroupVM> Regions { get; set; } = new();
    public VietnamMapVM Map { get; set; } = new();
    public List<RelicTypeLinkVM> RelicTypes { get; set; } = new();
}
