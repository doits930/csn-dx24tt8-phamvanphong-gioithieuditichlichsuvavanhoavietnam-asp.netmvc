using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Relics;

public class RelicService : IRelicService
{
    private static readonly StringComparer VietnameseComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: true);

    private readonly AppDbContext _context;
    private readonly IProvinceService _provinceService;
    private readonly IRelicTypeService _relicTypeService;

    public RelicService(
        AppDbContext context,
        IProvinceService provinceService,
        IRelicTypeService relicTypeService)
    {
        _context = context;
        _provinceService = provinceService;
        _relicTypeService = relicTypeService;
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

        var heroImages = await LoadImagesAsync(heroIds);

        return new HomeShowcaseVM
        {
            HeroSlides = hero
                .Select(r => RelicFactory.ToHeroSlideVM(
                    r,
                    heroImages.TryGetValue(r.Id, out var images) ? images : Array.Empty<RelicImage>()))
                .ToList(),
            Featured = featured.Select(RelicFactory.ToCardVM).ToList()
        };
    }

    private async Task<Dictionary<int, IReadOnlyList<RelicImage>>> LoadImagesAsync(IReadOnlyCollection<int> relicIds)
    {
        if (relicIds.Count == 0)
        {
            return new Dictionary<int, IReadOnlyList<RelicImage>>();
        }

        var images = await _context.RelicImages
            .AsNoTracking()
            .Where(i => relicIds.Contains(i.RelicId))
            .OrderBy(i => i.Id)
            .ToListAsync();

        return images
            .GroupBy(i => i.RelicId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<RelicImage>)g.ToList());
    }

    public async Task<SearchBarVM> GetSearchBarAsync()
    {
        var provinceGroups = await _provinceService.GetGroupedByRegionAsync();
        var relicTypes = await _relicTypeService.GetAllWithCountAsync();

        return SearchBarFactory.Build(new SearchFilterVM(), provinceGroups, relicTypes);
    }

    public async Task<RelicListVM?> SearchAsync(SearchFilterVM filter)
    {
        var provinceGroups = await _provinceService.GetGroupedByRegionAsync();
        var relicTypes = await _relicTypeService.GetAllWithCountAsync();

        var provinces = provinceGroups.SelectMany(g => g.Provinces).ToList();

        ProvinceLinkVM? selectedProvince = null;
        if (!string.IsNullOrWhiteSpace(filter.ProvinceSlug))
        {
            selectedProvince = provinces.FirstOrDefault(p => p.Slug == filter.ProvinceSlug);
            if (selectedProvince is null)
            {
                return null;
            }
        }

        RelicTypeLinkVM? selectedType = null;
        if (!string.IsNullOrWhiteSpace(filter.TypeSlug))
        {
            selectedType = relicTypes.FirstOrDefault(t => t.Slug == filter.TypeSlug);
            if (selectedType is null)
            {
                return null;
            }
        }

        if (filter.Ranking.HasValue && !Enum.IsDefined(filter.Ranking.Value))
        {
            filter.Ranking = null;
        }

        var keyword = SlugHelper.RemoveDiacritics(filter.NormalizedKeyword);
        var query = RelicSearchQuery.ApplyFilter(_context.Relics.AsNoTracking(), filter, keyword);

        var totalCount = await query.CountAsync();

        var sortKeys = await query
            .Select(r => new RelicSortKey(
                r.Id,
                r.Name,
                keyword.Length == 0 || r.NameNoAccent.Contains(keyword)))
            .ToListAsync();

        var orderedIds = sortKeys
            .OrderByDescending(k => k.MatchesName)
            .ThenBy(k => k.Name, VietnameseComparer)
            .Select(k => k.Id)
            .ToList();

        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)SearchFilterVM.PageSize));
        var currentPage = Math.Min(filter.Page, totalPages);
        filter.Page = currentPage;

        var pageIds = orderedIds
            .Skip((currentPage - 1) * SearchFilterVM.PageSize)
            .Take(SearchFilterVM.PageSize)
            .ToList();

        var pageRelics = await _context.Relics
            .AsNoTracking()
            .Include(r => r.Province)
            .Include(r => r.RelicType)
            .Where(r => pageIds.Contains(r.Id))
            .ToListAsync();

        var relicById = pageRelics.ToDictionary(r => r.Id);
        var items = new List<RelicCardVM>();
        foreach (var id in pageIds)
        {
            if (!relicById.TryGetValue(id, out var relic))
            {
                continue;
            }

            var matchesName = keyword.Length == 0 || relic.NameNoAccent.Contains(keyword);
            var excerpt = matchesName ? null : RelicSearchQuery.BuildMatchExcerpt(relic.Description, keyword);
            items.Add(RelicFactory.ToCardVM(relic, excerpt));
        }

        var model = new RelicListVM
        {
            Items = items,
            TotalCount = totalCount,
            Filter = filter,
            PageTitle = BuildPageTitle(filter, selectedProvince, selectedType),
            HeaderNote = BuildHeaderNote(filter, totalCount, provinceGroups, selectedProvince, selectedType),
            FilterSummary = BuildFilterSummary(filter, totalCount, selectedProvince, selectedType),
            BasePath = RelicSearchQuery.BasePath(filter),
            ClearFilterUrl = RelicListPaths.AllRelics,
            Breadcrumbs = BuildBreadcrumbs(filter, selectedProvince, selectedType),
            SearchBar = SearchBarFactory.Build(filter, provinceGroups, relicTypes),
            Pagination = new PaginationVM
            {
                CurrentPage = currentPage,
                TotalPages = totalPages,
                BasePath = RelicSearchQuery.BasePath(filter),
                RouteValues = RelicSearchQuery.QueryValues(filter)
            }
        };

        ApplySuggestions(model, provinceGroups, relicTypes, selectedProvince, selectedType);

        return model;
    }

    private const int RelatedRelicCount = 3;

    public async Task<RelicDetailVM?> GetDetailAsync(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var relic = await _context.Relics
            .AsNoTracking()
            .Include(r => r.Province)
            .Include(r => r.RelicType)
            .FirstOrDefaultAsync(r => r.Slug == slug);

        if (relic is null)
        {
            return null;
        }

        var images = await _context.RelicImages
            .AsNoTracking()
            .Where(i => i.RelicId == relic.Id)
            .OrderBy(i => i.Id)
            .ToListAsync();

        var model = RelicFactory.ToDetailVM(relic, images);

        model.RelatedSameProvince = await LoadRelatedAsync(
            _context.Relics.Where(r => r.ProvinceId == relic.ProvinceId),
            relic.Id,
            Array.Empty<int>());

        var excluded = model.RelatedSameProvince.Select(card => card.Id).ToList();
        model.RelatedSameType = await LoadRelatedAsync(
            _context.Relics.Where(r => r.RelicTypeId == relic.RelicTypeId),
            relic.Id,
            excluded);

        return model;
    }

    private async Task<List<RelicCardVM>> LoadRelatedAsync(
        IQueryable<Relic> source,
        int currentRelicId,
        IReadOnlyCollection<int> excludedIds)
    {
        var related = await source
            .AsNoTracking()
            .Where(r => r.Id != currentRelicId && !excludedIds.Contains(r.Id))
            .Include(r => r.Province)
            .Include(r => r.RelicType)
            .OrderByDescending(r => r.RankingLevel)
            .ThenByDescending(r => r.ViewCount)
            .ThenBy(r => r.Id)
            .Take(RelatedRelicCount)
            .ToListAsync();

        return related.Select(RelicFactory.ToCardVM).ToList();
    }

    public async Task<bool> IncreaseViewCountAsync(int relicId)
    {
        var changed = await _context.Relics
            .Where(r => r.Id == relicId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.ViewCount, r => r.ViewCount + 1));

        return changed > 0;
    }

    private const int SparseResultThreshold = 2;
    private const int MaxSuggestionLinks = 6;

    private static void ApplySuggestions(
        RelicListVM model,
        List<RegionGroupVM> provinceGroups,
        List<RelicTypeLinkVM> relicTypes,
        ProvinceLinkVM? selectedProvince,
        RelicTypeLinkVM? selectedType)
    {
        if (model.TotalCount > SparseResultThreshold || model.Pagination.CurrentPage > 1)
        {
            return;
        }

        if (selectedProvince is not null)
        {
            var region = provinceGroups.FirstOrDefault(g => g.Provinces.Any(p => p.Slug == selectedProvince.Slug));
            if (region is null)
            {
                return;
            }

            model.SuggestionTitle = $"Tỉnh thành khác ở {region.DisplayName}";
            model.SuggestionLinks = region.Provinces
                .Where(p => p.Slug != selectedProvince.Slug && p.RelicCount > 0)
                .OrderByDescending(p => p.RelicCount)
                .ThenBy(p => p.Name, VietnameseComparer)
                .Take(MaxSuggestionLinks)
                .Select(p => new SuggestionLinkVM
                {
                    Label = p.Name,
                    Url = RelicListPaths.ByProvince(p.Slug),
                    RelicCount = p.RelicCount
                })
                .ToList();
            return;
        }

        model.SuggestionTitle = "Duyệt theo loại di tích";
        model.SuggestionLinks = relicTypes
            .Where(t => t.RelicCount > 0 && t.Slug != selectedType?.Slug)
            .Select(t => new SuggestionLinkVM
            {
                Label = t.Name,
                Url = RelicListPaths.ByType(t.Slug),
                RelicCount = t.RelicCount
            })
            .ToList();
    }

    private static List<BreadcrumbItemVM> BuildBreadcrumbs(
        SearchFilterVM filter,
        ProvinceLinkVM? province,
        RelicTypeLinkVM? type)
    {
        var trail = new List<BreadcrumbItemVM>
        {
            new() { Label = "Trang chủ", Url = "/" }
        };

        var hasLeaf = !string.IsNullOrWhiteSpace(filter.NormalizedKeyword) || province is not null || type is not null;
        trail.Add(new BreadcrumbItemVM
        {
            Label = "Di tích",
            Url = hasLeaf ? RelicListPaths.AllRelics : null
        });

        if (!hasLeaf)
        {
            return trail;
        }

        if (!string.IsNullOrWhiteSpace(filter.NormalizedKeyword))
        {
            trail.Add(new BreadcrumbItemVM { Label = "Kết quả tìm kiếm" });
        }
        else if (province is not null && !LeadsWithType(filter, type))
        {
            trail.Add(new BreadcrumbItemVM { Label = province.Name });
        }
        else if (type is not null)
        {
            trail.Add(new BreadcrumbItemVM { Label = type!.Name });
        }

        return trail;
    }

    private static string BuildFilterSummary(
        SearchFilterVM filter,
        int totalCount,
        ProvinceLinkVM? province,
        RelicTypeLinkVM? type)
    {
        if (!filter.HasAnyFilter)
        {
            return string.Empty;
        }

        var clauses = new List<string>();
        if (!string.IsNullOrWhiteSpace(filter.NormalizedKeyword))
        {
            clauses.Add($"cho từ khóa “{filter.NormalizedKeyword}”");
        }

        if (province is not null)
        {
            clauses.Add($"tại {province.Name}");
        }

        if (type is not null)
        {
            clauses.Add($"loại {type.Name}");
        }

        if (filter.Ranking.HasValue)
        {
            clauses.Add(RankingLevelText.SummaryPhrase(filter.Ranking.Value));
        }

        var opening = totalCount > 0
            ? $"{totalCount} di tích"
            : "Không có di tích nào";

        return $"{opening} {string.Join(", ", clauses)}";
    }

    private static string? BuildHeaderNote(
        SearchFilterVM filter,
        int totalCount,
        List<RegionGroupVM> provinceGroups,
        ProvinceLinkVM? province,
        RelicTypeLinkVM? type)
    {
        if (!filter.HasAnyFilter)
        {
            if (totalCount == 0)
            {
                return "Chưa có di tích nào để hiển thị.";
            }

            var provinceCount = provinceGroups.SelectMany(g => g.Provinces).Count(p => p.RelicCount > 0);
            return $"Hiện có {totalCount} di tích, thuộc {provinceCount} tỉnh thành.";
        }

        if (!string.IsNullOrWhiteSpace(filter.NormalizedKeyword))
        {
            return null;
        }

        if (province is not null && !LeadsWithType(filter, type))
        {
            var region = provinceGroups.FirstOrDefault(g => g.Provinces.Any(p => p.Slug == province.Slug));
            return region is null ? null : $"{province.Name} thuộc {region.DisplayName}.";
        }

        return filter.TypeFromRoute ? type?.Description : null;
    }

    private static bool LeadsWithType(SearchFilterVM filter, RelicTypeLinkVM? type) =>
        filter.TypeFromRoute && type is not null;

    private static string BuildPageTitle(SearchFilterVM filter, ProvinceLinkVM? province, RelicTypeLinkVM? type)
    {
        var keyword = filter.NormalizedKeyword;
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            return $"Kết quả cho “{keyword}”";
        }

        if (province is not null && !LeadsWithType(filter, type))
        {
            return $"Di tích tại {province.Name}";
        }

        if (type is not null)
        {
            return type.Name;
        }

        return "Tất cả di tích";
    }

    private record RelicSortKey(int Id, string Name, bool MatchesName);

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
