namespace DiTichVietNam.Web.Services.Images;

public static class ImageFileValidator
{
    public const string EmptyFile = "Tệp rỗng, không có dữ liệu ảnh.";
    public const string TooSmall = "Tệp quá nhỏ để là một tấm ảnh thật.";
    public const string TooLarge = "Tệp nặng hơn mức cho phép " + ImageUploadLimits.MaxFileSizeText + ".";
    public const string BadExtension = "Chỉ nhận tệp " + ImageUploadLimits.AllowedExtensionText + ".";
    public const string NotAnImage = "Nội dung tệp không phải ảnh JPEG, PNG hay WEBP.";
    public const string ExtensionMismatch = "Đuôi tệp không khớp với định dạng ảnh bên trong.";
    public const string BrokenImage = "Tệp ảnh thiếu phần kết, có thể đã hỏng khi sao chép.";

    public static ImageFileCheck Check(string? fileName, long length, ReadOnlySpan<byte> header, ReadOnlySpan<byte> tail)
    {
        if (length <= 0)
        {
            return ImageFileCheck.Reject(EmptyFile);
        }

        var extension = ReadExtension(fileName);
        if (!ImageUploadLimits.AllowedExtensions.Contains(extension))
        {
            return ImageFileCheck.Reject(BadExtension);
        }

        if (length < ImageUploadLimits.MinFileBytes)
        {
            return ImageFileCheck.Reject(TooSmall);
        }

        if (length > ImageUploadLimits.MaxFileBytes)
        {
            return ImageFileCheck.Reject(TooLarge);
        }

        var signature = ReadSignature(header);
        if (signature.Length == 0)
        {
            return ImageFileCheck.Reject(NotAnImage);
        }

        if (!MatchesExtension(signature, extension))
        {
            return ImageFileCheck.Reject(ExtensionMismatch);
        }

        if (signature == ".jpg" && !EndsWithJpegMarker(tail))
        {
            return ImageFileCheck.Reject(BrokenImage);
        }

        return ImageFileCheck.Pass(signature);
    }

    public static string ReadSignature(ReadOnlySpan<byte> header)
    {
        if (IsJpeg(header))
        {
            return ".jpg";
        }

        if (IsPng(header))
        {
            return ".png";
        }

        return IsWebp(header) ? ".webp" : string.Empty;
    }

    private static bool MatchesExtension(string signature, string extension) => signature switch
    {
        ".jpg" => extension is ".jpg" or ".jpeg",
        ".png" => extension == ".png",
        ".webp" => extension == ".webp",
        _ => false
    };

    private static string ReadExtension(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var trimmed = fileName.Trim();
        var lastDot = trimmed.LastIndexOf('.');

        return lastDot < 0 ? string.Empty : trimmed[lastDot..].ToLowerInvariant();
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header) =>
        header.Length >= 4 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF
        && header[3] >= 0xC0;

    private static bool IsPng(ReadOnlySpan<byte> header) =>
        header.Length >= 16
        && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
        && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A
        && header[12] == 0x49 && header[13] == 0x48 && header[14] == 0x44 && header[15] == 0x52;

    private static bool IsWebp(ReadOnlySpan<byte> header) =>
        header.Length >= 12
        && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
        && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

    private static bool EndsWithJpegMarker(ReadOnlySpan<byte> tail)
    {
        var last = tail.Length - 1;
        while (last >= 0 && tail[last] == 0x00)
        {
            last--;
        }

        return last >= 1 && tail[last] == 0xD9 && tail[last - 1] == 0xFF;
    }
}
