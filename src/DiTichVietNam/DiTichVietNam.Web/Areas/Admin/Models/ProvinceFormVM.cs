namespace DiTichVietNam.Web.Areas.Admin.Models;

public class ProvinceFormVM
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Region { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public bool HasMapShape { get; set; }
    public string? ReturnUrl { get; set; }

    public bool IsEdit => Id > 0;
}
