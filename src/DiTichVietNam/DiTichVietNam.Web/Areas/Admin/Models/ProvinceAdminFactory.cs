using DiTichVietNam.Web.Services.Provinces;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class ProvinceAdminFactory
{
    public static ProvinceListVM ToListVM(ProvinceAdminListResult result, string? keyword, string listUrl) => new()
    {
        Rows = result.Rows,
        Summary = result.Summary,
        Keyword = keyword?.Trim(),
        ListUrl = listUrl,
        ResultSummary = BuildResultSummary(result, keyword)
    };

    public static ProvinceFormVM NewForm(string? returnUrl) => new()
    {
        ReturnUrl = returnUrl
    };

    public static ProvinceFormVM ToFormVM(ProvinceEditData data, string? returnUrl) => new()
    {
        Id = data.Id,
        Name = data.Name,
        Region = data.Region,
        Slug = data.Slug,
        RelicCount = data.RelicCount,
        HasMapShape = data.HasMapShape,
        ReturnUrl = returnUrl
    };

    public static ProvinceDeleteVM ToDeleteVM(ProvinceEditData data, string? returnUrl) => new()
    {
        Id = data.Id,
        Name = data.Name,
        Slug = data.Slug,
        RelicCount = data.RelicCount,
        BlockReason = ProvinceFactory.DeleteBlockReason(data.RelicCount, data.HasMapShape),
        ReturnUrl = returnUrl
    };

    public static ProvinceInput ToInput(ProvinceFormVM vm) => new()
    {
        Name = vm.Name,
        Region = vm.Region
    };

    private static string BuildResultSummary(ProvinceAdminListResult result, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return $"{result.Summary.TotalCount} tỉnh thành trong danh mục.";
        }

        return result.MatchedCount == 0
            ? $"Không có tỉnh thành nào khớp từ khóa trong {result.Summary.TotalCount} tỉnh thành."
            : $"{result.MatchedCount} trong {result.Summary.TotalCount} tỉnh thành khớp từ khóa.";
    }
}
