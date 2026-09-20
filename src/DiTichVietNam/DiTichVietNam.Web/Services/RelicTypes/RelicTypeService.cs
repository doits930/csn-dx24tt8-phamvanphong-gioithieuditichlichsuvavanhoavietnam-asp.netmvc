using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.RelicTypes;

public class RelicTypeService : IRelicTypeService
{
    private const int MaxSlugLength = 120;

    private const string NotFoundMessage = "Không tìm thấy loại di tích này.";
    private const string DuplicateNameMessage = "Tên loại di tích này đã có trong danh mục.";
    private const string NameJustTakenMessage =
        "Tên loại di tích này vừa được dùng cho một bản ghi khác. Chọn tên khác rồi lưu lại.";

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

    public async Task<RelicTypeAdminListResult> GetAdminListAsync()
    {
        var rows = await _context.RelicTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new CountedRelicType(t, t.Relics.Count))
            .ToListAsync();

        var items = rows
            .Select(r => RelicTypeFactory.ToAdminRow(r.RelicType, r.RelicCount))
            .ToList();

        return new RelicTypeAdminListResult
        {
            Rows = items,
            Summary = new RelicTypeAdminSummary
            {
                TotalCount = items.Count,
                WithRelicCount = items.Count(t => t.RelicCount > 0),
                WithoutRelicCount = items.Count(t => t.RelicCount == 0),
                RelicTotal = items.Sum(t => t.RelicCount)
            }
        };
    }

    public async Task<RelicTypeEditData?> GetForEditAsync(int id)
    {
        var row = await _context.RelicTypes
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new CountedRelicType(t, t.Relics.Count))
            .FirstOrDefaultAsync();

        return row is null ? null : RelicTypeFactory.ToEditData(row.RelicType, row.RelicCount);
    }

    public async Task<ServiceResult<int>> CreateAsync(RelicTypeInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (SlugHelper.ToSlug(name).Length == 0)
        {
            return ServiceResult<int>.Fail(RelicTypeFormValidator.NameWithoutLetter, nameof(RelicTypeInput.Name));
        }

        if (await IsNameTakenAsync(name, null))
        {
            return ServiceResult<int>.Fail(DuplicateNameMessage, nameof(RelicTypeInput.Name));
        }

        var slug = await BuildUniqueSlugAsync(name);
        if (slug.Length == 0)
        {
            return ServiceResult<int>.Fail(DuplicateNameMessage, nameof(RelicTypeInput.Name));
        }

        var relicType = RelicTypeFactory.ToEntity(input, slug);
        _context.RelicTypes.Add(relicType);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
        {
            _context.Entry(relicType).State = EntityState.Detached;

            return ServiceResult<int>.Fail(NameJustTakenMessage, nameof(RelicTypeInput.Name));
        }

        return ServiceResult<int>.Ok(relicType.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, RelicTypeInput input)
    {
        var relicType = await _context.RelicTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (relicType is null)
        {
            return ServiceResult.Fail(NotFoundMessage);
        }

        var name = input.Name?.Trim() ?? string.Empty;
        if (await IsNameTakenAsync(name, id))
        {
            return ServiceResult.Fail(DuplicateNameMessage, nameof(RelicTypeInput.Name));
        }

        relicType.Name = name;
        relicType.Description = RelicTypeFactory.NormalizeDescription(input.Description);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
        {
            await _context.Entry(relicType).ReloadAsync();

            return ServiceResult.Fail(NameJustTakenMessage, nameof(RelicTypeInput.Name));
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var relicType = await _context.RelicTypes.FirstOrDefaultAsync(t => t.Id == id);
        if (relicType is null)
        {
            return ServiceResult.Fail(NotFoundMessage);
        }

        var relicCount = await _context.Relics.CountAsync(r => r.RelicTypeId == id);
        var blockReason = RelicTypeFactory.DeleteBlockReason(relicCount);
        if (blockReason is not null)
        {
            return ServiceResult.Fail(blockReason);
        }

        _context.RelicTypes.Remove(relicType);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task<bool> IsNameTakenAsync(string name, int? currentTypeId)
    {
        var target = SlugHelper.ToSlug(name);
        if (target.Length == 0)
        {
            return false;
        }

        var names = await _context.RelicTypes
            .AsNoTracking()
            .Where(t => currentTypeId == null || t.Id != currentTypeId)
            .Select(t => t.Name)
            .ToListAsync();

        return names.Any(existing => SlugHelper.ToSlug(existing) == target);
    }

    private async Task<string> BuildUniqueSlugAsync(string name)
    {
        var baseSlug = LimitSlugLength(SlugHelper.ToSlug(name));
        if (baseSlug.Length == 0)
        {
            return string.Empty;
        }

        if (!await SlugTakenAsync(baseSlug))
        {
            return baseSlug;
        }

        for (var suffix = 2; suffix < 100; suffix++)
        {
            var candidate = LimitSlugLength($"{baseSlug}-{suffix}");
            if (!await SlugTakenAsync(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }

    private Task<bool> SlugTakenAsync(string slug) =>
        _context.RelicTypes.AnyAsync(t => t.Slug == slug);

    private static string LimitSlugLength(string slug) =>
        slug.Length <= MaxSlugLength ? slug : slug[..MaxSlugLength].TrimEnd('-');

    private record CountedRelicType(RelicType RelicType, int RelicCount);
}
