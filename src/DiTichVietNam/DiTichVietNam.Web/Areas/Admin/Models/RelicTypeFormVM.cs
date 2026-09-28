namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicTypeFormVM
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public string? ReturnUrl { get; set; }

    public bool IsEdit => Id > 0;
}
