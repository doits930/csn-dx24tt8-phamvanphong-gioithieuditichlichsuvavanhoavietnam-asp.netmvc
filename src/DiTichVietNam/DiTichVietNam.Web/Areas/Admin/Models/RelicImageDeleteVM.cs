using DiTichVietNam.Web.Services.Images;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicImageDeleteVM
{
    public int RelicId { get; set; }
    public string RelicName { get; set; } = string.Empty;
    public RelicImageItem Image { get; set; } = new();
    public string BoardUrl { get; set; } = string.Empty;

    public string DeleteUrl => $"/admin/di-tich/{RelicId}/anh/{Image.Id}/xoa";
}
