namespace DiTichVietNam.Web.Models.ViewModels;

public class HomeShowcaseVM
{
    public List<RelicCardVM> HeroHighlights { get; set; } = new();
    public List<RelicCardVM> Featured { get; set; } = new();
}
