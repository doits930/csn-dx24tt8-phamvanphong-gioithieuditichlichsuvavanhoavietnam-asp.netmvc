namespace DiTichVietNam.Web.Models.ViewModels;

public class VietnamMapVM
{
    public string ViewBox { get; set; } = string.Empty;
    public List<ProvinceMapCellVM> Provinces { get; set; } = new();
    public List<MapArchipelagoVM> Archipelagos { get; set; } = new();
    public List<MapIslandVM> Islands { get; set; } = new();
    public List<MapLegendStepVM> LegendSteps { get; set; } = new();
}
