namespace DiTichVietNam.Web.Services.Images;

public static class ImageDetailValidator
{
    public const string SourceRequired = "Nhập nguồn ảnh.";
    public const string SourceTooLong = "Nguồn ảnh tối đa 500 ký tự.";
    public const string CaptionTooLong = "Chú thích tối đa 300 ký tự.";

    public const string CaptionField = "Caption";
    public const string SourceField = "ImageSource";

    public static List<(string Field, string Message)> Validate(string? caption, string? source)
    {
        var errors = new List<(string Field, string Message)>();

        if (caption?.Trim().Length > ImageUploadLimits.MaxCaptionLength)
        {
            errors.Add((CaptionField, CaptionTooLong));
        }

        var trimmedSource = source?.Trim();
        if (string.IsNullOrEmpty(trimmedSource))
        {
            errors.Add((SourceField, SourceRequired));
        }
        else if (trimmedSource.Length > ImageUploadLimits.MaxSourceLength)
        {
            errors.Add((SourceField, SourceTooLong));
        }

        return errors;
    }
}
