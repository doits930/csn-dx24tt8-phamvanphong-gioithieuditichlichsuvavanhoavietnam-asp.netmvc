using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Provinces;

public interface IProvinceService
{
    Task<List<RegionGroupVM>> GetGroupedByRegionAsync();
    Task<VietnamMapVM> GetVietnamMapAsync();
}
