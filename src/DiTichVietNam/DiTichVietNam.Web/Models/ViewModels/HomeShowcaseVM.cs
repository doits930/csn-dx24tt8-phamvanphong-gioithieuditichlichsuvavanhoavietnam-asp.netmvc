namespace DiTichVietNam.Web.Models.ViewModels;

public class HomeShowcaseVM
{
    public List<HeroSlideVM> HeroSlides { get; set; } = new();
    public List<RelicCardVM> Featured { get; set; } = new();
}
