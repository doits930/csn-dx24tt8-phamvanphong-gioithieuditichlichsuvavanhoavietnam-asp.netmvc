using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.RelicTypes;

public interface IRelicTypeService
{
    Task<List<RelicTypeLinkVM>> GetAllWithCountAsync();
}
