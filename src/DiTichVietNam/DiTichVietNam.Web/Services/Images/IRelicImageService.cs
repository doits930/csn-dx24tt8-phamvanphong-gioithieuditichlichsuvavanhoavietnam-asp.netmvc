using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Images;

public interface IRelicImageService
{
    Task<RelicImageBoard?> GetBoardAsync(int relicId);

    Task<ImageUploadReport> UploadBatchAsync(int relicId, IReadOnlyList<RelicImageUpload> uploads);

    Task<ServiceResult> UpdateDetailAsync(int relicId, int imageId, string? caption, string? source);

    Task<ServiceResult> SetThumbnailAsync(int relicId, int imageId);

    Task<ServiceResult<string>> DeleteAsync(int relicId, int imageId);
}
