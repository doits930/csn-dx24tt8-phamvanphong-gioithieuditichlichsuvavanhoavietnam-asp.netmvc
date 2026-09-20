namespace DiTichVietNam.Web.Services.Images;

public class ImageFileCheck
{
    public bool Accepted { get; private set; }
    public string Extension { get; private set; } = string.Empty;
    public string? Reason { get; private set; }

    public static ImageFileCheck Pass(string extension) => new()
    {
        Accepted = true,
        Extension = extension
    };

    public static ImageFileCheck Reject(string reason) => new()
    {
        Accepted = false,
        Reason = reason
    };
}
