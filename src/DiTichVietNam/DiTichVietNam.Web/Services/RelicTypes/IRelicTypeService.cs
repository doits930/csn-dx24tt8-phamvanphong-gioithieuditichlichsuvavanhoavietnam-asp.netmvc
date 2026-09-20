using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.RelicTypes;

public interface IRelicTypeService
{
    Task<List<RelicTypeLinkVM>> GetAllWithCountAsync();

    Task<RelicTypeAdminListResult> GetAdminListAsync();

    Task<RelicTypeEditData?> GetForEditAsync(int id);

    Task<ServiceResult<int>> CreateAsync(RelicTypeInput input);

    Task<ServiceResult> UpdateAsync(int id, RelicTypeInput input);

    Task<ServiceResult> DeleteAsync(int id);
}
