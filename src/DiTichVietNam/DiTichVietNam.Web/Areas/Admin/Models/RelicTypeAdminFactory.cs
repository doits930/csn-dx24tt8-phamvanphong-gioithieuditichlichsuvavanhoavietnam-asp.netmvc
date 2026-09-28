using DiTichVietNam.Web.Services.RelicTypes;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class RelicTypeAdminFactory
{
    public static RelicTypeListVM ToListVM(RelicTypeAdminListResult result) => new()
    {
        Rows = result.Rows,
        Summary = result.Summary,
        ResultSummary = $"{result.Summary.TotalCount} loại di tích trong danh mục."
    };

    public static RelicTypeFormVM NewForm(string? returnUrl) => new()
    {
        ReturnUrl = returnUrl
    };

    public static RelicTypeFormVM ToFormVM(RelicTypeEditData data, string? returnUrl) => new()
    {
        Id = data.Id,
        Name = data.Name,
        Description = data.Description,
        Slug = data.Slug,
        RelicCount = data.RelicCount,
        ReturnUrl = returnUrl
    };

    public static RelicTypeDeleteVM ToDeleteVM(RelicTypeEditData data, string? returnUrl) => new()
    {
        Id = data.Id,
        Name = data.Name,
        Slug = data.Slug,
        RelicCount = data.RelicCount,
        BlockReason = RelicTypeFactory.DeleteBlockReason(data.RelicCount),
        ReturnUrl = returnUrl
    };

    public static RelicTypeInput ToInput(RelicTypeFormVM vm) => new()
    {
        Name = vm.Name,
        Description = vm.Description
    };
}
