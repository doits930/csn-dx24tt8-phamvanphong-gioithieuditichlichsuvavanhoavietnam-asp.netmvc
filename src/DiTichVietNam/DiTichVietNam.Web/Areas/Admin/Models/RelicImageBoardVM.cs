using DiTichVietNam.Web.Services.Images;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicImageBoardVM
{
    public RelicImageBoard Board { get; set; } = new();

    public string ListUrl { get; set; } = string.Empty;
    public string UploadUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;

    public int? EditingImageId { get; set; }
    public string? EditCaption { get; set; }
    public string? EditSource { get; set; }
    public string? EditCaptionError { get; set; }
    public string? EditSourceError { get; set; }

    public ImageUploadReport? LastReport { get; set; }

    public string ImageActionUrl(int imageId, string action) =>
        $"/admin/di-tich/{Board.RelicId}/anh/{imageId}/{action}";
}
