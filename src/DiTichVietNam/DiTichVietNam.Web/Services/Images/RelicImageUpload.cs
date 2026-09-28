namespace DiTichVietNam.Web.Services.Images;

public class RelicImageUpload
{
    public IFormFile File { get; set; } = default!;
    public string? Caption { get; set; }
    public string? Source { get; set; }
}
