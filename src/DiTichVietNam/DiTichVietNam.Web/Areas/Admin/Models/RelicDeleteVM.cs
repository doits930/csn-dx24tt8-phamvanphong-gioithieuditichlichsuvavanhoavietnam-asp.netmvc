namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicDeleteVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ImageCount { get; set; }
    public string? ReturnUrl { get; set; }
}
