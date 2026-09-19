using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Provinces;

public class ProvinceService : IProvinceService
{
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

    private async Task<List<CountedProvince>> LoadCountedProvincesAsync() =>
        await _context.Provinces
            .Select(p => new CountedProvince(p, p.Relics.Count))
            .ToListAsync();

    private record CountedProvince(Province Province, int RelicCount);
}
