using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Provinces;

public interface IProvinceService
{
    Task<List<RegionGroupVM>> GetGroupedByRegionAsync();

    Task<VietnamMapVM> GetVietnamMapAsync();

    Task<ProvinceAdminListResult> GetAdminListAsync(string? keyword);

    Task<ProvinceEditData?> GetForEditAsync(int id);

    Task<ServiceResult<int>> CreateAsync(ProvinceInput input);

    Task<ServiceResult> UpdateAsync(int id, ProvinceInput input);

    Task<ServiceResult> DeleteAsync(int id);
}
