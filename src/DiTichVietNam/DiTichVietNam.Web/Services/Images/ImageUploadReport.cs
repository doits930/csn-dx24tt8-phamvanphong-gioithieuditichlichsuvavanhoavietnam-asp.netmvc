namespace DiTichVietNam.Web.Services.Images;

public class ImageUploadReport
{
    public int SavedCount { get; set; }
    public string? Error { get; set; }
    public List<ImageUploadRejection> Rejections { get; set; } = new();

    public bool HasRejections => Rejections.Count > 0;
}
