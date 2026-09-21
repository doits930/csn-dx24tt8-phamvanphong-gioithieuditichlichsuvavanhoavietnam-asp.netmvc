using System.Globalization;
using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Relics;

public class RelicService : IRelicService
{
    private const int MinSuggestionLength = 2;
    private const int MaxSuggestionCount = 10;

    private static readonly StringComparer VietnameseComparer =
        StringComparer.Create(CultureInfo.GetCultureInfo("vi-VN"), ignoreCase: true);

    private readonly AppDbContext _context;
    private readonly IProvinceService _provinceService;
    private readonly IRelicTypeService _relicTypeService;
    private readonly IWebHostEnvironment _environment;

    public RelicService(
        AppDbContext context,
        IProvinceService provinceService,
        IRelicTypeService relicTypeService,
        IWebHostEnvironment environment)
    {
        _context = context;
        _provinceService = provinceService;
        _relicTypeService = relicTypeService;
        _environment = environment;
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

    public Task<int> CountAsync() => _context.Relics.CountAsync();

    public async Task<RelicAdminListResult> SearchForAdminAsync(SearchFilterVM filter, int pageSize)
    {
        if (pageSize < 1)
        {
            pageSize = SearchFilterVM.PageSize;
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

        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var currentPage = Math.Min(filter.Page, totalPages);
        filter.Page = currentPage;

        var pageIds = orderedIds
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var pageRelics = await _context.Relics
            .AsNoTracking()
            .Include(r => r.Province)
            .Include(r => r.RelicType)
            .Where(r => pageIds.Contains(r.Id))
            .ToListAsync();

        var imageCounts = await _context.RelicImages
            .AsNoTracking()
            .Where(i => pageIds.Contains(i.RelicId))
            .GroupBy(i => i.RelicId)
            .Select(g => new { RelicId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.RelicId, g => g.Count);

        var relicById = pageRelics.ToDictionary(r => r.Id);
        var rows = new List<RelicAdminRow>();
        foreach (var id in pageIds)
        {
            if (!relicById.TryGetValue(id, out var relic))
            {
                continue;
            }

            rows.Add(RelicFactory.ToAdminRow(relic, imageCounts.TryGetValue(id, out var count) ? count : 0));
        }

        return new RelicAdminListResult
        {
            Rows = rows,
            TotalCount = totalCount,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            Options = await GetFormOptionsAsync(),
            Summary = await GetAdminSummaryAsync()
        };
    }

    public async Task<List<RelicSuggestion>> SuggestForAdminAsync(string? keyword, int limit)
    {
        if (limit < 1)
        {
            limit = 1;
        }
        else if (limit > MaxSuggestionCount)
        {
            limit = MaxSuggestionCount;
        }

        var trimmed = keyword?.Trim();
        if (trimmed is not null && trimmed.Length > SearchFilterVM.MaxSuggestionKeywordLength)
        {
            return new List<RelicSuggestion>();
        }

        var normalized = SlugHelper.RemoveDiacritics(trimmed);
        if (normalized.Length < MinSuggestionLength)
        {
            return new List<RelicSuggestion>();
        }

        var matches = await _context.Relics
            .AsNoTracking()
            .Where(r => r.NameNoAccent.Contains(normalized))
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.NameNoAccent,
                r.ThumbnailPath,
                ProvinceName = r.Province!.Name
            })
            .ToListAsync();

        return matches
            .OrderByDescending(r => r.NameNoAccent.StartsWith(normalized, StringComparison.Ordinal))
            .ThenBy(r => r.Name, VietnameseComparer)
            .Take(limit)
            .Select(r => new RelicSuggestion
            {
                Id = r.Id,
                Name = r.Name,
                ProvinceName = r.ProvinceName,
                ThumbnailPath = r.ThumbnailPath
            })
            .ToList();
    }

    public async Task<RelicAdminSummary> GetAdminSummaryAsync() => new()
    {
        TotalCount = await _context.Relics.CountAsync(),
        SpecialNationalCount = await _context.Relics.CountAsync(r => r.RankingLevel == RankingLevel.SpecialNational),
        WithoutImageCount = await _context.Relics.CountAsync(r => !r.Images.Any()),
        TotalViewCount = await _context.Relics.SumAsync(r => r.ViewCount)
    };

    public async Task<RelicFormOptions> GetFormOptionsAsync()
    {
        var provinces = await _context.Provinces
            .AsNoTracking()
            .Select(p => new { p.Id, p.Name, p.Slug, p.Region })
            .ToListAsync();

        var options = new RelicFormOptions();

        foreach (var region in RegionText.OrderedRegions)
        {
            var items = provinces
                .Where(p => p.Region == region)
                .OrderBy(p => p.Name, VietnameseComparer)
                .Select(p => new RelicOptionItem { Id = p.Id, Name = p.Name, Slug = p.Slug })
                .ToList();

            if (items.Count > 0)
            {
                options.ProvinceGroups.Add(new RelicOptionGroup
                {
                    Label = RegionText.DisplayName(region),
                    Items = items
                });
            }
        }

        options.RelicTypes = await _context.RelicTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new RelicOptionItem { Id = t.Id, Name = t.Name, Slug = t.Slug })
            .ToListAsync();

        return options;
    }

    public async Task<RelicEditData?> GetForEditAsync(int id)
    {
        var relic = await _context.Relics
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (relic is null)
        {
            return null;
        }

        var imageCount = await _context.RelicImages.CountAsync(i => i.RelicId == id);

        return RelicFactory.ToEditData(relic, imageCount);
    }

    public async Task<ServiceResult<int>> CreateAsync(RelicInput input)
    {
        var reference = await CheckReferencesAsync(input);
        if (reference is not null)
        {
            return ServiceResult<int>.Fail(reference.Value.Message, reference.Value.Field);
        }

        var slug = await BuildUniqueSlugAsync(input.Name, input.ProvinceId, null);
        if (slug.Length == 0)
        {
            return ServiceResult<int>.Fail(NameWithoutSlugMessage, nameof(RelicInput.Name));
        }

        var relic = RelicFactory.ToEntity(input);
        relic.Slug = slug;
        relic.CreatedAt = DateTime.Now;
        relic.ViewCount = 0;

        _context.Relics.Add(relic);

        for (var attempt = 0; attempt < MaxSlugConflictRetries; attempt++)
        {
            try
            {
                await _context.SaveChangesAsync();

                return ServiceResult<int>.Ok(relic.Id);
            }
            catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
            {
                var retrySlug = await BuildUniqueSlugAsync(input.Name, input.ProvinceId, null);
                if (retrySlug.Length == 0 || retrySlug == relic.Slug)
                {
                    break;
                }

                relic.Slug = retrySlug;
            }
        }

        _context.Entry(relic).State = EntityState.Detached;

        return ServiceResult<int>.Fail(SlugConflictMessage, nameof(RelicInput.Name));
    }

    public async Task<ServiceResult> UpdateAsync(int id, RelicInput input)
    {
        var relic = await _context.Relics.FirstOrDefaultAsync(r => r.Id == id);
        if (relic is null)
        {
            return ServiceResult.Fail(RelicMissingMessage);
        }

        var reference = await CheckReferencesAsync(input);
        if (reference is not null)
        {
            return ServiceResult.Fail(reference.Value.Message, reference.Value.Field);
        }

        var newSlug = relic.Slug;
        if (!string.Equals(relic.Name, input.Name.Trim(), StringComparison.Ordinal))
        {
            newSlug = await BuildUniqueSlugAsync(input.Name, input.ProvinceId, id);
            if (newSlug.Length == 0)
            {
                return ServiceResult.Fail(NameWithoutSlugMessage, nameof(RelicInput.Name));
            }
        }

        RelicFactory.ApplyToEntity(input, relic);
        relic.Slug = newSlug;
        relic.UpdatedAt = DateTime.Now;

        for (var attempt = 0; attempt < MaxSlugConflictRetries; attempt++)
        {
            try
            {
                await _context.SaveChangesAsync();

                return ServiceResult.Ok();
            }
            catch (DbUpdateException exception) when (DatabaseConflict.IsUniqueViolation(exception))
            {
                var retrySlug = await BuildUniqueSlugAsync(input.Name, input.ProvinceId, id);
                if (retrySlug.Length == 0 || retrySlug == relic.Slug)
                {
                    break;
                }

                relic.Slug = retrySlug;
            }
        }

        await _context.Entry(relic).ReloadAsync();

        return ServiceResult.Fail(SlugConflictMessage, nameof(RelicInput.Name));
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var relic = await _context.Relics.FirstOrDefaultAsync(r => r.Id == id);
        if (relic is null)
        {
            return ServiceResult.Fail(RelicMissingMessage);
        }

        _context.Relics.Remove(relic);
        await _context.SaveChangesAsync();

        return RemoveUploadFolder(id);
    }

    private ServiceResult RemoveUploadFolder(int relicId)
    {
        var root = _environment.WebRootPath;
        if (string.IsNullOrEmpty(root))
        {
            return ServiceResult.Ok();
        }

        var folder = Path.Combine(root, "uploads", "relics", relicId.ToString(CultureInfo.InvariantCulture));
        if (!Directory.Exists(folder))
        {
            return ServiceResult.Ok();
        }

        try
        {
            Directory.Delete(folder, recursive: true);
            return ServiceResult.Ok();
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return ServiceResult.Fail(UploadFolderLeftMessage);
        }
    }

    private async Task<(string Field, string Message)?> CheckReferencesAsync(RelicInput input)
    {
        if (!await _context.Provinces.AnyAsync(p => p.Id == input.ProvinceId))
        {
            return (nameof(RelicInput.ProvinceId), "Tỉnh thành vừa chọn không còn trong danh mục.");
        }

        if (!await _context.RelicTypes.AnyAsync(t => t.Id == input.RelicTypeId))
        {
            return (nameof(RelicInput.RelicTypeId), "Loại di tích vừa chọn không còn trong danh mục.");
        }

        if (!Enum.IsDefined(input.RankingLevel))
        {
            return (nameof(RelicInput.RankingLevel), "Chọn một trong ba cấp xếp hạng.");
        }

        return null;
    }

    private async Task<string> BuildUniqueSlugAsync(string name, int provinceId, int? currentRelicId)
    {
        var baseSlug = SlugHelper.ToSlug(name);
        if (baseSlug.Length == 0)
        {
            return string.Empty;
        }

        baseSlug = LimitSlugLength(baseSlug);
        if (!await SlugTakenAsync(baseSlug, currentRelicId))
        {
            return baseSlug;
        }

        var provinceSlug = await _context.Provinces
            .Where(p => p.Id == provinceId)
            .Select(p => p.Slug)
            .FirstOrDefaultAsync();

        var candidate = baseSlug;
        if (!string.IsNullOrWhiteSpace(provinceSlug))
        {
            candidate = LimitSlugLength($"{baseSlug}-{provinceSlug}");
            if (!await SlugTakenAsync(candidate, currentRelicId))
            {
                return candidate;
            }
        }

        var suffix = 2;
        while (true)
        {
            var numbered = LimitSlugLength($"{candidate}-{suffix}");
            if (!await SlugTakenAsync(numbered, currentRelicId))
            {
                return numbered;
            }

            suffix++;
        }
    }

    private Task<bool> SlugTakenAsync(string slug, int? currentRelicId) =>
        _context.Relics.AnyAsync(r => r.Slug == slug && (currentRelicId == null || r.Id != currentRelicId));

    private static string LimitSlugLength(string slug) =>
        slug.Length <= MaxSlugLength ? slug : slug[..MaxSlugLength].TrimEnd('-');

    private const int MaxSlugLength = 200;

    private const int MaxSlugConflictRetries = 5;

    private const string NameWithoutSlugMessage =
        "Tên di tích phải có ít nhất một chữ cái hoặc chữ số để tạo được địa chỉ trang.";

    private const string SlugConflictMessage =
        "Có di tích khác vừa lấy mất địa chỉ trang sinh từ tên này. Bấm lưu lại một lần nữa.";

    private const string RelicMissingMessage = "Di tích này không còn trong danh sách.";

    private const string UploadFolderLeftMessage =
        "Đã xóa di tích, nhưng thư mục ảnh của di tích chưa xóa được. Hãy kiểm tra lại thư mục uploads.";

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
