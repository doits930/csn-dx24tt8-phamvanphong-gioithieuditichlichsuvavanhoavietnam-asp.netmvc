namespace DiTichVietNam.Web.Models.ViewModels;

public class RegionGroupVM
{
    public string DisplayName { get; set; } = string.Empty;
    public List<ProvinceLinkVM> Provinces { get; set; } = new();
}
