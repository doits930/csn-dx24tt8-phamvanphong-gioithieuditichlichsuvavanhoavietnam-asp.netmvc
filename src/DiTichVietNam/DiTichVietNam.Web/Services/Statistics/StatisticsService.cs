using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Statistics;

public class StatisticsService : IStatisticsService
{
    private const int TopProvinceCount = 10;
    private const int MostViewedCount = 8;
    private const int RecentlyChangedCount = 5;
    private const int MissingSampleCount = 3;

    private static readonly RankingLevel[] OrderedRankings =
    {
        RankingLevel.SpecialNational,
        RankingLevel.National,
        RankingLevel.Provincial
    };

    private static readonly StringComparer VietnameseComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: true);

    private readonly AppDbContext _context;

    public StatisticsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatistics> GetDashboardAsync()
    {
        var relics = _context.Relics.AsNoTracking();

        var totals = await relics
            .GroupBy(r => 1)
            .Select(g => new { RelicCount = g.Count(), ViewCount = g.Sum(r => r.ViewCount) })
            .FirstOrDefaultAsync();

        var statistics = new DashboardStatistics
        {
            RelicCount = totals?.RelicCount ?? 0,
            ViewCount = totals?.ViewCount ?? 0,
            ImageCount = await _context.RelicImages.AsNoTracking().CountAsync()
        };

        var provinces = await BuildProvincesAsync();
        statistics.ProvinceCount = provinces.Count;
        statistics.ProvinceWithRelicCount = provinces.Count(p => p.Count > 0);

        statistics.Rankings = await BuildRankingsAsync(statistics.RelicCount);
        statistics.Types = await BuildTypesAsync(statistics.RelicCount);

        statistics.TopProvinces = TakeWithTies(provinces.Where(p => p.Count > 0).ToList(), TopProvinceCount);
        StatisticsFactory.ApplyShares(statistics.TopProvinces, statistics.RelicCount);
        statistics.OtherProvinceCount = statistics.ProvinceWithRelicCount - statistics.TopProvinces.Count;

        statistics.Regions = BuildRegions(provinces, statistics.RelicCount);

        statistics.MostViewed = await relics
            .Where(r => r.ViewCount > 0)
            .OrderByDescending(r => r.ViewCount)
            .ThenBy(r => r.Id)
            .Take(MostViewedCount)
            .Select(r => new RelicBrief
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                ProvinceName = r.Province!.Name,
                ThumbnailPath = r.ThumbnailPath,
                ViewCount = r.ViewCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        statistics.RecentlyChanged = await relics
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .Take(RecentlyChangedCount)
            .Select(r => new RelicBrief
            {
                Id = r.Id,
                Name = r.Name,
                Slug = r.Slug,
                ProvinceName = r.Province!.Name,
                ThumbnailPath = r.ThumbnailPath,
                ViewCount = r.ViewCount,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        statistics.Missing = BuildMissingGroups(await LoadMissingCandidatesAsync());

        return statistics;
    }

    private static List<CountShare> TakeWithTies(List<CountShare> ordered, int limit)
    {
        if (ordered.Count <= limit)
        {
            return ordered;
        }

        var lastCount = ordered[limit - 1].Count;
        var taken = ordered.Take(limit).ToList();
        taken.AddRange(ordered.Skip(limit).TakeWhile(item => item.Count == lastCount));

        return taken;
    }

    private async Task<List<CountShare>> BuildRankingsAsync(int relicCount)
    {
        var rows = await _context.Relics
            .AsNoTracking()
            .GroupBy(r => r.RankingLevel)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToListAsync();

        var items = OrderedRankings
            .Select(level => new CountShare
            {
                Label = RankingLevelText.ShortLabel(level),
                Key = ((int)level).ToString(CultureInfo.InvariantCulture),
                Modifier = RankingLevelText.CssModifier(level),
                Count = rows.FirstOrDefault(row => row.Level == level)?.Count ?? 0
            })
            .ToList();

        StatisticsFactory.ApplyShares(items, relicCount);

        return items;
    }

    private async Task<List<CountShare>> BuildTypesAsync(int relicCount)
    {
        var rows = await _context.RelicTypes
            .AsNoTracking()
            .Select(t => new { t.Name, t.Slug, Count = t.Relics.Count() })
            .ToListAsync();

        var items = rows
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Name, VietnameseComparer)
            .Select(row => new CountShare
            {
                Label = row.Name,
                Key = row.Slug,
                Count = row.Count
            })
            .ToList();

        StatisticsFactory.ApplyShares(items, relicCount);

        return items;
    }

    private async Task<List<CountShare>> BuildProvincesAsync()
    {
        var rows = await _context.Provinces
            .AsNoTracking()
            .Select(p => new { p.Name, p.Slug, p.Region, Count = p.Relics.Count() })
            .ToListAsync();

        return rows
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Name, VietnameseComparer)
            .Select(row => new CountShare
            {
                Label = row.Name,
                Key = row.Slug,
                Modifier = row.Region,
                Count = row.Count
            })
            .ToList();
    }

    private static List<CountShare> BuildRegions(List<CountShare> provinces, int relicCount)
    {
        var items = RegionText.OrderedRegions
            .Select(region => new CountShare
            {
                Label = RegionText.DisplayName(region),
                Count = provinces.Where(p => p.Modifier == region).Sum(p => p.Count)
            })
            .ToList();

        StatisticsFactory.ApplyShares(items, relicCount);

        return items;
    }

    private async Task<List<MissingDataCandidate>> LoadMissingCandidatesAsync()
    {
        return await _context.Relics
            .AsNoTracking()
            .Where(r => !r.Images.Any()
                || r.Images.Count == 1
                || r.Latitude == null
                || r.Longitude == null
                || r.VisitInfo == null
                || r.VisitInfo.Trim() == "")
            .Select(r => new MissingDataCandidate
            {
                Relic = new RelicBrief
                {
                    Id = r.Id,
                    Name = r.Name,
                    Slug = r.Slug,
                    ProvinceName = r.Province!.Name,
                    ThumbnailPath = r.ThumbnailPath,
                    ViewCount = r.ViewCount,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                },
                WithoutImage = !r.Images.Any(),
                WithSingleImage = r.Images.Count == 1,
                WithoutLocation = (r.Latitude == null || r.Longitude == null)
                    && r.RelicType!.Slug != IntangibleHeritage.TypeSlug
                    && r.RelicType!.Name != IntangibleHeritage.TypeName,
                WithoutVisitInfo = r.VisitInfo == null || r.VisitInfo.Trim() == ""
            })
            .ToListAsync();
    }

    private static List<MissingDataGroup> BuildMissingGroups(List<MissingDataCandidate> candidates)
    {
        var ordered = candidates
            .OrderBy(candidate => candidate.Relic.Name, VietnameseComparer)
            .ToList();

        return new List<MissingDataGroup>
        {
            BuildMissingGroup(ordered, "anh", "Chưa có ảnh", "image",
                "Mọi mục đều đã có ảnh.", candidate => candidate.WithoutImage),
            BuildMissingGroup(ordered, "toa-do", "Chưa có tọa độ", "compass",
                "Mọi mục cần định vị đều đã có tọa độ.", candidate => candidate.WithoutLocation,
                "Không tính bản ghi thuộc loại di sản văn hóa phi vật thể vì nhóm này không định vị theo một điểm."),
            BuildMissingGroup(ordered, "mot-anh", "Chỉ có một ảnh", "upload",
                "Không mục nào chỉ có một ảnh.", candidate => candidate.WithSingleImage),
            BuildMissingGroup(ordered, "tham-quan", "Chưa có thông tin tham quan", "text",
                "Mọi mục đều đã có thông tin tham quan.", candidate => candidate.WithoutVisitInfo)
        };
    }

    private static MissingDataGroup BuildMissingGroup(
        List<MissingDataCandidate> ordered,
        string key,
        string label,
        string icon,
        string clearMessage,
        Func<MissingDataCandidate, bool> match,
        string? note = null)
    {
        var matched = ordered.Where(match).ToList();

        return new MissingDataGroup
        {
            Key = key,
            Label = label,
            Icon = icon,
            ClearMessage = clearMessage,
            Note = note,
            Count = matched.Count,
            Samples = matched.Take(MissingSampleCount).Select(candidate => candidate.Relic).ToList()
        };
    }
}
