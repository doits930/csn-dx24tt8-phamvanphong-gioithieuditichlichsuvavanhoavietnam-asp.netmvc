using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.RelicTypes;

public static class RelicTypeFactory
{
    public static RelicTypeLinkVM ToLinkVM(RelicType relicType, int relicCount, string? coverImagePath = null) => new()
    {
        Name = relicType.Name,
        Slug = relicType.Slug,
        Description = relicType.Description,
        RelicCount = relicCount,
        CoverImagePath = coverImagePath
    };

    public static RelicTypeAdminRow ToAdminRow(RelicType relicType, int relicCount) => new()
    {
        Id = relicType.Id,
        Name = relicType.Name,
        Slug = relicType.Slug,
        Description = relicType.Description,
        RelicCount = relicCount
    };

    public static RelicTypeEditData ToEditData(RelicType relicType, int relicCount) => new()
    {
        Id = relicType.Id,
        Name = relicType.Name,
        Slug = relicType.Slug,
        Description = relicType.Description,
        RelicCount = relicCount
    };

    public static RelicType ToEntity(RelicTypeInput input, string slug) => new()
    {
        Name = input.Name?.Trim() ?? string.Empty,
        Slug = slug,
        Description = NormalizeDescription(input.Description)
    };

    public static string? NormalizeDescription(string? description)
    {
        var text = description?.Trim();
        return string.IsNullOrEmpty(text) ? null : text;
    }

    public static string? DeleteBlockReason(int relicCount) =>
        relicCount > 0
            ? $"Không xóa được loại di tích này vì còn {relicCount} di tích đang dùng nó. "
                + "Chuyển những di tích đó sang loại khác rồi xóa lại."
            : null;
}
