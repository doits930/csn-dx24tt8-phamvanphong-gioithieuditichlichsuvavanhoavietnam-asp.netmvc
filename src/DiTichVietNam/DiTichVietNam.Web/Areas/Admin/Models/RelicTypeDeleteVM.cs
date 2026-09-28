namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicTypeDeleteVM
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public string? BlockReason { get; set; }
    public string? ReturnUrl { get; set; }

    public bool IsBlocked => BlockReason is not null;
}
