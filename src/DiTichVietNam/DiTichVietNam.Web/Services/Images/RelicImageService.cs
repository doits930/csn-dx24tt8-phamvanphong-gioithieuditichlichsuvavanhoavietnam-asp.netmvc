using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Images;

public class RelicImageService : IRelicImageService
{
    public const string RelicMissing = "Di tích này không còn trong danh sách.";
    public const string ImageMissing = "Ảnh này không còn trong danh sách.";
    public const string NoFileChosen = "Chọn ít nhất một tệp ảnh để tải lên.";
    public const string SaveFailed = "Không lưu được dữ liệu ảnh nên các tệp vừa ghi đã được xóa. Hãy thử lại.";
    public const string FileLeftBehind = "Đã xóa bản ghi ảnh, nhưng tệp trên đĩa chưa xóa được.";
    public const string WriteFailed = "Không ghi được tệp ảnh lên đĩa.";

    private const string UploadFolderName = "uploads";
    private const string RelicFolderName = "relics";

    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly RelicImageWriteLock _writeLock;

    public RelicImageService(
        AppDbContext context,
        IWebHostEnvironment environment,
        RelicImageWriteLock writeLock)
    {
        _context = context;
        _environment = environment;
        _writeLock = writeLock;
    }

    public static string TooManyFilesMessage =>
        $"Mỗi lần chỉ tải được tối đa {ImageUploadLimits.MaxFilesPerBatch} tệp. Hãy chia thành nhiều lượt.";

    public static string BatchTooLargeMessage =>
        $"Cả lượt không quá {ImageUploadLimits.MaxBatchSizeText}. Hãy chia lô ảnh thành nhiều lượt.";

    public static string RelicFullMessage =>
        $"Di tích đã có đủ {ImageUploadLimits.MaxImagesPerRelic} ảnh. Xóa bớt ảnh cũ trước khi thêm ảnh mới.";

    public async Task<RelicImageBoard?> GetBoardAsync(int relicId)
    {
        var relic = await _context.Relics
            .AsNoTracking()
            .Where(r => r.Id == relicId)
            .Select(r => new { r.Id, r.Name, r.Slug })
            .FirstOrDefaultAsync();

        if (relic is null)
        {
            return null;
        }

        var images = await _context.RelicImages
            .AsNoTracking()
            .Where(i => i.RelicId == relicId)
            .OrderBy(i => i.Id)
            .ToListAsync();

        var board = new RelicImageBoard
        {
            RelicId = relic.Id,
            RelicName = relic.Name,
            RelicSlug = relic.Slug
        };

        for (var index = 0; index < images.Count; index++)
        {
            board.Images.Add(RelicImageFactory.ToItem(images[index], relic.Name, index + 1));
        }

        return board;
    }

    public async Task<ImageUploadReport> UploadBatchAsync(int relicId, IReadOnlyList<RelicImageUpload> uploads)
    {
        var report = new ImageUploadReport();

        if (uploads.Count == 0)
        {
            report.Error = NoFileChosen;
            return report;
        }

        if (uploads.Count > ImageUploadLimits.MaxFilesPerBatch)
        {
            report.Error = TooManyFilesMessage;
            return report;
        }

        if (uploads.Sum(u => u.File.Length) > ImageUploadLimits.MaxBatchBytes)
        {
            report.Error = BatchTooLargeMessage;
            return report;
        }

        using var gate = await _writeLock.AcquireAsync(relicId);

        var relic = await _context.Relics.FirstOrDefaultAsync(r => r.Id == relicId);
        if (relic is null)
        {
            report.Error = RelicMissing;
            return report;
        }

        var existingCount = await _context.RelicImages.CountAsync(i => i.RelicId == relicId);
        if (existingCount >= ImageUploadLimits.MaxImagesPerRelic)
        {
            report.Error = RelicFullMessage;
            return report;
        }

        var folder = RelicFolderPath(relicId);
        var writtenFiles = new List<string>();
        var newImages = new List<RelicImage>();
        var remaining = ImageUploadLimits.MaxImagesPerRelic - existingCount;

        foreach (var upload in uploads)
        {
            var displayName = SafeDisplayName(upload.File.FileName);

            if (newImages.Count >= remaining)
            {
                report.Rejections.Add(Reject(displayName, RelicFullMessage));
                continue;
            }

            var check = await InspectFileAsync(upload.File);
            var reasons = new List<string>();

            if (!check.Accepted)
            {
                reasons.Add(check.Reason ?? ImageFileValidator.NotAnImage);
            }

            var detailErrors = ImageDetailValidator.Validate(upload.Caption, upload.Source);
            reasons.AddRange(detailErrors.Select(e => e.Message));

            if (reasons.Count > 0)
            {
                report.Rejections.Add(Reject(displayName, string.Join(" ", reasons)));
                continue;
            }

            var storedName = Guid.NewGuid().ToString("N") + check.Extension;
            var fullPath = Path.Combine(folder, storedName);

            try
            {
                Directory.CreateDirectory(folder);
                await using var target = File.Create(fullPath);
                await upload.File.CopyToAsync(target);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                report.Rejections.Add(Reject(displayName, WriteFailed));
                continue;
            }

            writtenFiles.Add(fullPath);
            newImages.Add(new RelicImage
            {
                RelicId = relicId,
                ImagePath = PublicPath(relicId, storedName),
                Caption = Normalize(upload.Caption),
                ImageSource = upload.Source!.Trim(),
                IsThumbnail = false
            });
        }

        if (newImages.Count == 0)
        {
            return report;
        }

        var hasThumbnail = await _context.RelicImages.AnyAsync(i => i.RelicId == relicId && i.IsThumbnail);
        if (!hasThumbnail)
        {
            newImages[0].IsThumbnail = true;
            relic.ThumbnailPath = newImages[0].ImagePath;
        }

        try
        {
            _context.RelicImages.AddRange(newImages);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            RemoveFiles(writtenFiles);
            report.Error = SaveFailed;
            report.SavedCount = 0;
            return report;
        }

        report.SavedCount = newImages.Count;
        return report;
    }

    public async Task<ServiceResult> UpdateDetailAsync(int relicId, int imageId, string? caption, string? source)
    {
        var image = await _context.RelicImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.RelicId == relicId);

        if (image is null)
        {
            return ServiceResult.Fail(ImageMissing);
        }

        var errors = ImageDetailValidator.Validate(caption, source);
        if (errors.Count > 0)
        {
            return ServiceResult.Fail(errors[0].Message, errors[0].Field);
        }

        image.Caption = Normalize(caption);
        image.ImageSource = source!.Trim();

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetThumbnailAsync(int relicId, int imageId)
    {
        var relic = await _context.Relics.FirstOrDefaultAsync(r => r.Id == relicId);
        if (relic is null)
        {
            return ServiceResult.Fail(RelicMissing);
        }

        var images = await _context.RelicImages
            .Where(i => i.RelicId == relicId)
            .ToListAsync();

        var target = images.FirstOrDefault(i => i.Id == imageId);
        if (target is null)
        {
            return ServiceResult.Fail(ImageMissing);
        }

        foreach (var image in images)
        {
            image.IsThumbnail = image.Id == imageId;
        }

        relic.ThumbnailPath = target.ImagePath;

        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<string>> DeleteAsync(int relicId, int imageId)
    {
        var relic = await _context.Relics.FirstOrDefaultAsync(r => r.Id == relicId);
        if (relic is null)
        {
            return ServiceResult<string>.Fail(RelicMissing);
        }

        var images = await _context.RelicImages
            .Where(i => i.RelicId == relicId)
            .OrderBy(i => i.Id)
            .ToListAsync();

        var target = images.FirstOrDefault(i => i.Id == imageId);
        if (target is null)
        {
            return ServiceResult<string>.Fail(ImageMissing);
        }

        var label = string.IsNullOrWhiteSpace(target.Caption)
            ? "ảnh"
            : $"ảnh “{target.Caption!.Trim()}”";

        _context.RelicImages.Remove(target);

        var next = images.FirstOrDefault(i => i.Id != imageId);
        if (target.IsThumbnail)
        {
            if (next is null)
            {
                relic.ThumbnailPath = null;
            }
            else
            {
                next.IsThumbnail = true;
                relic.ThumbnailPath = next.ImagePath;
            }
        }

        await _context.SaveChangesAsync();

        var filePath = ResolveUploadedFile(relicId, target.ImagePath);
        if (filePath is null)
        {
            RemoveEmptyFolder(relicId, next is null);
            return ServiceResult<string>.Ok(label);
        }

        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return ServiceResult<string>.Fail(FileLeftBehind, label);
        }

        RemoveEmptyFolder(relicId, next is null);

        return ServiceResult<string>.Ok(label);
    }

    private async Task<ImageFileCheck> InspectFileAsync(IFormFile file)
    {
        var header = new byte[ImageUploadLimits.HeaderByteCount];
        var tail = new byte[ImageUploadLimits.TailByteCount];
        var headerRead = 0;
        var tailRead = 0;

        if (file.Length > 0)
        {
            await using var stream = file.OpenReadStream();
            headerRead = await FillAsync(stream, header);

            if (stream.CanSeek && file.Length >= tail.Length)
            {
                stream.Seek(file.Length - tail.Length, SeekOrigin.Begin);
                tailRead = await FillAsync(stream, tail);
            }
        }

        return ImageFileValidator.Check(
            file.FileName,
            file.Length,
            header.AsSpan(0, headerRead),
            tail.AsSpan(0, tailRead));
    }

    private static async Task<int> FillAsync(Stream stream, byte[] buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var step = await stream.ReadAsync(buffer.AsMemory(read, buffer.Length - read));
            if (step == 0)
            {
                break;
            }

            read += step;
        }

        return read;
    }

    private void RemoveEmptyFolder(int relicId, bool lastImageRemoved)
    {
        if (!lastImageRemoved)
        {
            return;
        }

        var folder = RelicFolderPath(relicId);

        try
        {
            if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
            {
                Directory.Delete(folder);
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
        }
    }

    private static ImageUploadRejection Reject(string fileName, string reason) => new()
    {
        FileName = fileName,
        Reason = reason
    };

    private static string SafeDisplayName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Tệp không tên";
        }

        var name = Path.GetFileName(fileName.Trim().Replace('\\', '/'));

        return string.IsNullOrWhiteSpace(name) ? "Tệp không tên" : name;
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    private static string PublicPath(int relicId, string storedName) =>
        $"{RelicImageFactory.UploadPathPrefix}{relicId.ToString(CultureInfo.InvariantCulture)}/{storedName}";

    private string RelicFolderPath(int relicId) => Path.Combine(
        UploadRoot(),
        RelicFolderName,
        relicId.ToString(CultureInfo.InvariantCulture));

    private string UploadRoot()
    {
        var webRoot = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
        {
            webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        return Path.Combine(webRoot, UploadFolderName);
    }

    private string? ResolveUploadedFile(int relicId, string imagePath)
    {
        var expectedPrefix = $"{RelicImageFactory.UploadPathPrefix}{relicId.ToString(CultureInfo.InvariantCulture)}/";
        if (!imagePath.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            return null;
        }

        var storedName = imagePath[expectedPrefix.Length..];
        if (storedName.Length == 0 || storedName.Contains('/') || storedName.Contains('\\'))
        {
            return null;
        }

        var folder = Path.GetFullPath(RelicFolderPath(relicId));
        var candidate = Path.GetFullPath(Path.Combine(folder, storedName));

        return candidate.StartsWith(folder + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? candidate
            : null;
    }

    private static void RemoveFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
            }
        }
    }
}
