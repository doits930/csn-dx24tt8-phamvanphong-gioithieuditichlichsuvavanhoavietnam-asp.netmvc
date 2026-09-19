using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Provinces;

public static class ProvinceFactory
{
    public static ProvinceLinkVM ToLinkVM(Province province, int relicCount) => new()
    {
        Name = province.Name,
        Slug = province.Slug,
        RelicCount = relicCount
    };
}
