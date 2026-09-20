using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Provinces;

public class ProvinceService : IProvinceService
{
    private const int MaxSlugLength = 120;

    private const string NotFoundMessage = "Không tìm thấy tỉnh thành này.";
    private const string DuplicateNameMessage = "Tên tỉnh thành này đã có trong danh mục.";
    private const string NameJustTakenMessage =
        "Tên tỉnh thành này vừa được dùng cho một bản ghi khác. Chọn tên khác rồi lưu lại.";

    private static readonly StringComparer VietnameseComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: true);

    private readonly AppDbContext _context;

    public ProvinceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RegionGroupVM>> GetGroupedByRegionAsync()
    {
        var rows = await LoadCountedProvincesAsync();

        var groups = new List<RegionGroupVM>();
        foreach (var region in RegionText.OrderedRegions)
        {
            var provinces = rows
                .Where(r => r.Province.Region == region)
                .Select(r => ProvinceFactory.ToLinkVM(r.Province, r.RelicCount))
                .OrderBy(p => p.Name, VietnameseComparer)
                .ToList();

            if (provinces.Count > 0)
            {
                groups.Add(new RegionGroupVM
                {
                    DisplayName = RegionText.DisplayName(region),
                    Provinces = provinces
                });
            }
        }

        return groups;
    }

    public async Task<VietnamMapVM> GetVietnamMapAsync()
    {
        var rows = await LoadCountedProvincesAsync();

        var provincesBySlug = rows.ToDictionary(
            r => r.Province.Slug,
            r => ProvinceFactory.ToLinkVM(r.Province, r.RelicCount));

        return ProvinceFactory.ToMapVM(provincesBySlug);
    }

    public async Task<ProvinceAdminListResult> GetAdminListAsync(string? keyword)
    {
        var rows = await _context.Provinces
            .AsNoTracking()
            .Select(p => new CountedProvince(p, p.Relics.Count))
            .ToListAsync();

        var all = rows
            .Select(r => ProvinceFactory.ToAdminRow(r.Province, r.RelicCount))
            .ToList();

        var summary = new ProvinceAdminSummary
        {
            TotalCount = all.Count,
            WithRelicCount = all.Count(p => p.RelicCount > 0),
            WithoutRelicCount = all.Count(p => p.RelicCount == 0),
            WithoutMapCount = all.Count(p => !p.HasMapShape)
        };

        var matched = all;
        var normalized = SlugHelper.RemoveDiacritics(keyword);
        if (normalized.Length > 0)
        {
            matched = all
                .Where(p => p.NameNoAccent.Contains(normalized, StringComparison.Ordinal)
                    || p.Slug.Contains(normalized, StringComparison.Ordinal))
                .ToList();
        }

        return new ProvinceAdminListResult
        {
            Rows = matched.OrderBy(p => p.Name, VietnameseComparer).ToList(),
            Summary = summary,
            MatchedCount = matched.Count
        };
    }

    public async Task<ProvinceEditData?> GetForEditAsync(int id)
    {
        var row = await _context.Provinces
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new CountedProvince(p, p.Relics.Count))
            .FirstOrDefaultAsync();

        return row is null ? null : ProvinceFactory.ToEditData(row.Province, row.RelicCount);
    }

    public async Task<ServiceResult<int>> CreateAsync(ProvinceInput input)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        if (SlugHelper.ToSlug(name).Length == 0)
        {
            return ServiceResult<int>.Fail(ProvinceFormValidator.NameWithoutLetter, nameof(ProvinceInput.Name));
        }

        if (await IsNameTakenAsync(name, null))
        {
            return ServiceResult<int>.Fail(DuplicateNameMessage, nameof(ProvinceInput.Name));
        }

        var slug = await BuildUniqueSlugAsync(name);
        if (slug.Length == 0)
        {
            return ServiceResult<int>.Fail(DuplicateNameMessage, nameof(ProvinceInput.Name));
        }

        var province = ProvinceFactory.ToEntity(input, slug);
        _context.Provinces.Add(province);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
        {
            _context.Entry(province).State = EntityState.Detached;

            return ServiceResult<int>.Fail(NameJustTakenMessage, nameof(ProvinceInput.Name));
        }

        return ServiceResult<int>.Ok(province.Id);
    }

    public async Task<ServiceResult> UpdateAsync(int id, ProvinceInput input)
    {
        var province = await _context.Provinces.FirstOrDefaultAsync(p => p.Id == id);
        if (province is null)
        {
            return ServiceResult.Fail(NotFoundMessage);
        }

        var name = input.Name?.Trim() ?? string.Empty;
        if (await IsNameTakenAsync(name, id))
        {
            return ServiceResult.Fail(DuplicateNameMessage, nameof(ProvinceInput.Name));
        }

        province.Name = name;
        province.Region = input.Region?.Trim() ?? string.Empty;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
        {
            await _context.Entry(province).ReloadAsync();

            return ServiceResult.Fail(NameJustTakenMessage, nameof(ProvinceInput.Name));
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var province = await _context.Provinces.FirstOrDefaultAsync(p => p.Id == id);
        if (province is null)
        {
            return ServiceResult.Fail(NotFoundMessage);
        }

        var relicCount = await _context.Relics.CountAsync(r => r.ProvinceId == id);
        var blockReason = ProvinceFactory.DeleteBlockReason(
            relicCount,
            VietnamMapGeometry.HasProvinceShape(province.Slug));

        if (blockReason is not null)
        {
            return ServiceResult.Fail(blockReason);
        }

        _context.Provinces.Remove(province);
        await _context.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task<bool> IsNameTakenAsync(string name, int? currentProvinceId)
    {
        var target = SlugHelper.ToSlug(name);
        if (target.Length == 0)
        {
            return false;
        }

        var names = await _context.Provinces
            .AsNoTracking()
            .Where(p => currentProvinceId == null || p.Id != currentProvinceId)
            .Select(p => p.Name)
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
        _context.Provinces.AnyAsync(p => p.Slug == slug);

    private static string LimitSlugLength(string slug) =>
        slug.Length <= MaxSlugLength ? slug : slug[..MaxSlugLength].TrimEnd('-');

    private async Task<List<CountedProvince>> LoadCountedProvincesAsync() =>
        await _context.Provinces
            .Select(p => new CountedProvince(p, p.Relics.Count))
            .ToListAsync();

    private record CountedProvince(Province Province, int RelicCount);
}
