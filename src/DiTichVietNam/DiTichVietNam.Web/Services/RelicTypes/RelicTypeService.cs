using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.RelicTypes;

public class RelicTypeService : IRelicTypeService
{
    private readonly AppDbContext _context;

    public RelicTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RelicTypeLinkVM>> GetAllWithCountAsync()
    {
        var rows = await _context.RelicTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new
            {
                Type = t,
                RelicCount = t.Relics.Count,
                CoverImagePath = t.Relics
                    .Where(r => r.ThumbnailPath != null && r.ThumbnailPath != "")
                    .OrderBy(r => r.Id)
                    .Select(r => r.ThumbnailPath)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return rows
            .Select(r => RelicTypeFactory.ToLinkVM(r.Type, r.RelicCount, r.CoverImagePath))
            .ToList();
    }
}
