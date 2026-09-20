using System.Text.Json;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Data;

public static class SeedData
{
    public const string AdminRoleName = "Admin";
    public const string AdminEmail = "admin@ditich.vn";
    public const string AdminPassword = "Admin@123";

    private const int MaxNameLength = 200;
    private const int MaxSlugLength = 220;
    private const int MaxAddressLength = 300;
    private const int MaxCaptionLength = 300;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var environment = services.GetRequiredService<IWebHostEnvironment>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SeedData));

        await SeedRelicTypesAsync(context);
        await SeedProvincesAsync(context, environment, logger);
        await SeedRelicsAsync(context, environment, logger);
        await FillMissingDescriptionNoAccentAsync(context, logger);
        await SeedAdminAccountAsync(services, logger);
    }

    private static async Task FillMissingDescriptionNoAccentAsync(AppDbContext context, ILogger logger)
    {
        var pending = await context.Relics
            .Where(r => r.DescriptionNoAccent == "" && r.Description != "")
            .ToListAsync();

        if (pending.Count == 0)
        {
            return;
        }

        foreach (var relic in pending)
        {
            relic.DescriptionNoAccent = SlugHelper.RemoveDiacritics(relic.Description);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Đã tính phần mô tả không dấu cho {Count} di tích.", pending.Count);
    }

    private static async Task SeedRelicTypesAsync(AppDbContext context)
    {
        var definitions = new[]
        {
            new RelicType
            {
                Name = "Di tích lịch sử",
                Slug = "di-tich-lich-su",
                Description = "Công trình, địa điểm gắn với sự kiện lịch sử tiêu biểu hoặc thân thế, sự nghiệp của nhân vật lịch sử."
            },
            new RelicType
            {
                Name = "Di tích kiến trúc nghệ thuật",
                Slug = "di-tich-kien-truc-nghe-thuat",
                Description = "Công trình kiến trúc, tác phẩm điêu khắc và trang trí có giá trị tiêu biểu về nghệ thuật của một thời kỳ."
            },
            new RelicType
            {
                Name = "Di tích khảo cổ",
                Slug = "di-tich-khao-co",
                Description = "Địa điểm còn dấu vết cư trú, mộ táng hoặc hiện vật của người xưa, được nghiên cứu bằng phương pháp khảo cổ học."
            },
            new RelicType
            {
                Name = "Danh lam thắng cảnh",
                Slug = "danh-lam-thang-canh",
                Description = "Cảnh quan thiên nhiên hoặc địa điểm có sự kết hợp giữa cảnh quan thiên nhiên với công trình kiến trúc có giá trị."
            }
        };

        var existingSlugs = await context.RelicTypes.Select(t => t.Slug).ToListAsync();
        var missing = definitions.Where(t => !existingSlugs.Contains(t.Slug)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        context.RelicTypes.AddRange(missing);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProvincesAsync(AppDbContext context, IWebHostEnvironment environment, ILogger logger)
    {
        var items = ReadJsonFile<List<ProvinceSeed>>(environment, "provinces.json", logger);
        if (items is null || items.Count == 0)
        {
            return;
        }

        var existingSlugs = await context.Provinces.Select(p => p.Slug).ToListAsync();
        var added = new List<Province>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                continue;
            }

            var region = item.Region?.Trim();
            if (region is not ("Bắc" or "Trung" or "Nam"))
            {
                logger.LogWarning("Bỏ qua tỉnh thành {Name} vì miền không hợp lệ: {Region}.", item.Name, item.Region);
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(item.Slug) ? SlugHelper.ToSlug(item.Name) : item.Slug.Trim();
            if (existingSlugs.Contains(slug) || added.Any(p => p.Slug == slug))
            {
                continue;
            }

            added.Add(new Province
            {
                Name = item.Name.Trim(),
                Slug = slug,
                Region = region
            });
        }

        if (added.Count == 0)
        {
            return;
        }

        context.Provinces.AddRange(added);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã nạp {Count} tỉnh thành từ dữ liệu ban đầu.", added.Count);
    }

    private static async Task SeedRelicsAsync(AppDbContext context, IWebHostEnvironment environment, ILogger logger)
    {
        var file = ReadJsonFile<RelicSeedFile>(environment, "relics.json", logger);
        var items = file?.Relics;
        if (items is null || items.Count == 0)
        {
            return;
        }

        var provinceIdBySlug = await context.Provinces.ToDictionaryAsync(p => p.Slug, p => p.Id);
        var typeIdByCode = await GetRelicTypeIdByCodeAsync(context);
        var existingSlugs = await context.Relics.Select(r => r.Slug).ToListAsync();
        var added = new List<Relic>();

        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.ProvinceSlug))
            {
                logger.LogWarning("Bỏ qua một bản ghi di tích thiếu tên hoặc thiếu tỉnh thành.");
                continue;
            }

            var slug = string.IsNullOrWhiteSpace(item.Slug) ? SlugHelper.ToSlug(item.Name) : item.Slug.Trim();
            if (existingSlugs.Contains(slug) || added.Any(r => r.Slug == slug))
            {
                continue;
            }

            if (!provinceIdBySlug.TryGetValue(item.ProvinceSlug.Trim(), out var provinceId))
            {
                logger.LogWarning("Bỏ qua di tích {Slug} vì chưa có tỉnh thành {Province} trong danh mục.", slug, item.ProvinceSlug);
                continue;
            }

            if (!typeIdByCode.TryGetValue(item.RelicType, out var relicTypeId))
            {
                logger.LogWarning("Bỏ qua di tích {Slug} vì mã loại di tích {Type} không hợp lệ.", slug, item.RelicType);
                continue;
            }

            var violation = FindConstraintViolation(item, slug);
            if (violation is not null)
            {
                logger.LogWarning("Bỏ qua di tích {Slug} vì {Reason}.", slug, violation);
                continue;
            }

            var relic = new Relic
            {
                Name = item.Name.Trim(),
                Slug = slug,
                NameNoAccent = SlugHelper.RemoveDiacritics(item.Name),
                Address = item.Address?.Trim() ?? string.Empty,
                Latitude = item.Latitude,
                Longitude = item.Longitude,
                History = item.History,
                Description = item.Description!,
                DescriptionNoAccent = SlugHelper.RemoveDiacritics(item.Description),
                VisitInfo = item.VisitInfo,
                RankingLevel = ToRankingLevel(item.RankingLevel),
                RecognizedYear = item.RecognizedYear,
                SourceUrl = item.SourceUrl!.Trim(),
                ProvinceId = provinceId,
                RelicTypeId = relicTypeId,
                CreatedAt = DateTime.Now
            };

            AttachImages(relic, item.Images, environment, logger);
            added.Add(relic);
        }

        if (added.Count == 0)
        {
            return;
        }

        context.Relics.AddRange(added);
        await context.SaveChangesAsync();
        logger.LogInformation("Đã nạp {Count} di tích từ dữ liệu ban đầu.", added.Count);
    }

    private static string? FindConstraintViolation(RelicSeed item, string slug)
    {
        if (item.Name!.Trim().Length > MaxNameLength)
        {
            return $"tên dài quá {MaxNameLength} ký tự";
        }

        if (slug.Length > MaxSlugLength)
        {
            return $"slug dài quá {MaxSlugLength} ký tự";
        }

        if ((item.Address?.Trim().Length ?? 0) > MaxAddressLength)
        {
            return $"địa chỉ dài quá {MaxAddressLength} ký tự";
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            return "thiếu phần mô tả bắt buộc";
        }

        if (string.IsNullOrWhiteSpace(item.SourceUrl))
        {
            return "thiếu nguồn tham khảo bắt buộc";
        }

        if (SlugHelper.RemoveDiacritics(item.Name).Length > MaxNameLength)
        {
            return $"tên không dấu dài quá {MaxNameLength} ký tự";
        }

        return null;
    }

    private static void AttachImages(Relic relic, List<RelicImageSeed>? images, IWebHostEnvironment environment, ILogger logger)
    {
        if (images is null || images.Count == 0)
        {
            return;
        }

        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var accepted = new List<RelicImage>();

        foreach (var image in images)
        {
            var file = image.File?.Trim().Replace('\\', '/').TrimStart('/');
            if (string.IsNullOrWhiteSpace(file))
            {
                logger.LogWarning("Bỏ qua một ảnh của di tích {Slug} vì thiếu tên tệp.", relic.Slug);
                continue;
            }

            if (string.IsNullOrWhiteSpace(image.ImageSource))
            {
                logger.LogWarning("Bỏ qua ảnh {File} của di tích {Slug} vì thiếu nguồn ảnh.", file, relic.Slug);
                continue;
            }

            var physicalPath = Path.Combine(webRoot, "img", "relics", file.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(physicalPath))
            {
                logger.LogWarning("Bỏ qua ảnh {File} của di tích {Slug} vì không tìm thấy tệp trên đĩa.", file, relic.Slug);
                continue;
            }

            var caption = image.Caption?.Trim();
            if (caption is not null && caption.Length > MaxCaptionLength)
            {
                caption = null;
                logger.LogWarning("Chú thích ảnh {File} của di tích {Slug} dài quá {Limit} ký tự nên để trống.",
                    file, relic.Slug, MaxCaptionLength);
            }

            accepted.Add(new RelicImage
            {
                ImagePath = "/img/relics/" + file,
                Caption = caption,
                ImageSource = image.ImageSource.Trim(),
                IsThumbnail = image.IsThumbnail
            });
        }

        if (accepted.Count == 0)
        {
            logger.LogWarning("Di tích {Slug} không có ảnh hợp lệ nào.", relic.Slug);
            return;
        }

        var thumbnail = accepted.FirstOrDefault(i => i.IsThumbnail) ?? accepted[0];
        foreach (var image in accepted)
        {
            image.IsThumbnail = ReferenceEquals(image, thumbnail);
            relic.Images.Add(image);
        }

        relic.ThumbnailPath = thumbnail.ImagePath;
    }

    private static async Task<Dictionary<int, int>> GetRelicTypeIdByCodeAsync(AppDbContext context)
    {
        var slugByCode = new Dictionary<int, string>
        {
            [1] = "di-tich-lich-su",
            [2] = "di-tich-kien-truc-nghe-thuat",
            [3] = "di-tich-khao-co",
            [4] = "danh-lam-thang-canh"
        };

        var idBySlug = await context.RelicTypes.ToDictionaryAsync(t => t.Slug, t => t.Id);
        var result = new Dictionary<int, int>();
        foreach (var pair in slugByCode)
        {
            if (idBySlug.TryGetValue(pair.Value, out var id))
            {
                result[pair.Key] = id;
            }
        }

        return result;
    }

    private static RankingLevel ToRankingLevel(int value) => value switch
    {
        2 => RankingLevel.National,
        3 => RankingLevel.SpecialNational,
        _ => RankingLevel.Provincial
    };

    private static async Task SeedAdminAccountAsync(IServiceProvider services, ILogger logger)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        if (!await roleManager.RoleExistsAsync(AdminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(AdminRoleName));
        }

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var created = await userManager.CreateAsync(admin, AdminPassword);
            if (!created.Succeeded)
            {
                logger.LogError("Không tạo được tài khoản quản trị: {Errors}",
                    string.Join("; ", created.Errors.Select(e => e.Description)));
                return;
            }
        }
        else if (!admin.LockoutEnabled)
        {
            await userManager.SetLockoutEnabledAsync(admin, true);
        }

        if (!await userManager.IsInRoleAsync(admin, AdminRoleName))
        {
            await userManager.AddToRoleAsync(admin, AdminRoleName);
        }
    }

    private static T? ReadJsonFile<T>(IWebHostEnvironment environment, string fileName, ILogger logger)
    {
        var path = Path.Combine(environment.ContentRootPath, "Data", "Seed", fileName);
        if (!File.Exists(path))
        {
            return default;
        }

        var content = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Tệp dữ liệu ban đầu {File} sai định dạng, bỏ qua phần này.", fileName);
            return default;
        }
    }

    private class ProvinceSeed
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Region { get; set; }
    }

    private class RelicSeedFile
    {
        public List<RelicSeed>? Relics { get; set; }
    }

    private class RelicSeed
    {
        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? ProvinceSlug { get; set; }
        public int RelicType { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? History { get; set; }
        public string? Description { get; set; }
        public string? VisitInfo { get; set; }
        public int RankingLevel { get; set; }
        public int? RecognizedYear { get; set; }
        public string? SourceUrl { get; set; }
        public List<RelicImageSeed>? Images { get; set; }
    }

    private class RelicImageSeed
    {
        public string? File { get; set; }
        public string? Caption { get; set; }
        public string? ImageSource { get; set; }
        public bool IsThumbnail { get; set; }
    }
}
