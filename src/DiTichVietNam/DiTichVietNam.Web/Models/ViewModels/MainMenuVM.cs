namespace DiTichVietNam.Web.Models.ViewModels;

public class MainMenuVM
{
    public List<RegionGroupVM> Regions { get; set; } = new();
    public List<RelicTypeLinkVM> RelicTypes { get; set; } = new();
    public string? Keyword { get; set; }
    public bool ShowSearch { get; set; } = true;
}
