namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicImageUploadVM
{
    public List<IFormFile>? Files { get; set; }

    public List<string>? Captions { get; set; }

    public List<string>? Sources { get; set; }

    public string? SharedCaption { get; set; }

    public string? SharedSource { get; set; }
}
