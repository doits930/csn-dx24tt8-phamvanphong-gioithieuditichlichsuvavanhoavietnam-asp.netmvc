namespace DiTichVietNam.Web.Models.ViewModels;

public class MainMenuVM
{
    public List<RegionGroupVM> Regions { get; set; } = new();
    public List<RelicTypeLinkVM> RelicTypes { get; set; } = new();
    public string? Keyword { get; set; }
    public bool ShowSearch { get; set; } = true;
    public bool IsAdminSignedIn { get; set; }
    public string? AccountName { get; set; }
    public string AdminHomeUrl { get; set; } = string.Empty;
    public string LogoutUrl { get; set; } = string.Empty;
}
