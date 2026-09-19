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
}
