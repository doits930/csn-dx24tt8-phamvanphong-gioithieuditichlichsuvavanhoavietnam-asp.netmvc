using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Relics;

public class RelicService : IRelicService
{
    private readonly AppDbContext _context;

    public RelicService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HomeShowcaseVM> GetHomeShowcaseAsync(int heroCount, int featuredCount)
    {
        if (heroCount < 0 || featuredCount < 0)
        {
            return new HomeShowcaseVM();
        }

        var candidates = await _context.Relics
            .AsNoTracking()
            .Include(r => r.Province)
            .Include(r => r.RelicType)
            .OrderByDescending(r => r.ViewCount)
            .ThenByDescending(r => r.RankingLevel)
            .ThenByDescending(r => r.CreatedAt)
            .Take(heroCount + featuredCount + MaxProvinceSpreadLookahead)
            .ToListAsync();

        var hero = PickOnePerProvince(candidates, heroCount);
        var heroIds = hero.Select(r => r.Id).ToHashSet();

        var remaining = candidates.Where(r => !heroIds.Contains(r.Id)).ToList();
        var featured = PickOnePerProvince(remaining, featuredCount);
        var featuredIds = featured.Select(r => r.Id).ToHashSet();
        featured.AddRange(remaining
            .Where(r => !featuredIds.Contains(r.Id))
            .Take(featuredCount - featured.Count));

        return new HomeShowcaseVM
        {
            HeroHighlights = hero.Select(RelicFactory.ToCardVM).ToList(),
            Featured = featured.Select(RelicFactory.ToCardVM).ToList()
        };
    }

    public async Task<HomeStatsVM> GetHomeStatsAsync()
    {
        return new HomeStatsVM
        {
            RelicCount = await _context.Relics.CountAsync(),
            ProvinceCount = await _context.Provinces.CountAsync(),
            ProvinceHavingRelicCount = await _context.Provinces.CountAsync(p => p.Relics.Any()),
            RelicTypeCount = await _context.RelicTypes.CountAsync()
        };
    }

    private const int MaxProvinceSpreadLookahead = 60;

    private static List<Relic> PickOnePerProvince(List<Relic> ordered, int count)
    {
        var picked = new List<Relic>();
        var seenProvinces = new HashSet<int>();

        foreach (var relic in ordered)
        {
            if (picked.Count >= count)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(relic.ThumbnailPath))
            {
                continue;
            }

            if (!seenProvinces.Add(relic.ProvinceId))
            {
                continue;
            }

            picked.Add(relic);
        }

        return picked;
    }
}
