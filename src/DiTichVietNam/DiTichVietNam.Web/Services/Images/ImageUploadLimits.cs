namespace DiTichVietNam.Web.Services.Images;

public static class ImageUploadLimits
{
    public const int MaxFilesPerBatch = 10;
    public const long MinFileBytes = 128;
    public const long MaxFileBytes = 5L * 1024 * 1024;
    public const long MaxBatchBytes = 30L * 1024 * 1024;
    public const long MaxRequestBytes = 64L * 1024 * 1024;
    public const int MaxImagesPerRelic = 30;
    public const int MaxCaptionLength = 300;
    public const int MaxSourceLength = 500;
    public const int HeaderByteCount = 16;
    public const int TailByteCount = 16;

    public const string MaxFileSizeText = "5 MB";
    public const string MaxBatchSizeText = "30 MB";
    public const string AllowedExtensionText = ".jpg, .jpeg, .png, .webp";
    public const string AcceptAttribute = ".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp";

    public static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
}
