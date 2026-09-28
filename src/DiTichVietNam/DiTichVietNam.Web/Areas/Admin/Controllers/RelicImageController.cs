using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Services.Accounts;
using DiTichVietNam.Web.Services.Images;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class RelicImageController : AdminBaseController
{
    public const string BoardPath = "/admin/di-tich/{relicId:int}/anh";
    public const string UploadPath = "/admin/di-tich/{relicId:int}/anh/tai-len";
    public const string DetailPath = "/admin/di-tich/{relicId:int}/anh/{imageId:int}/sua";
    public const string ThumbnailPath = "/admin/di-tich/{relicId:int}/anh/{imageId:int}/dat-dai-dien";
    public const string DeletePath = "/admin/di-tich/{relicId:int}/anh/{imageId:int}/xoa";

    private readonly IRelicImageService _imageService;
    private readonly AdminSiteOptions _adminSite;

    public RelicImageController(IRelicImageService imageService, IOptions<AdminSiteOptions> adminSite)
    {
        _imageService = imageService;
        _adminSite = adminSite.Value;
    }

    [HttpGet(BoardPath)]
    public async Task<IActionResult> Index([FromRoute] int relicId)
    {
        var vm = await BuildBoardAsync(relicId);
        if (vm is null)
        {
            return AdminNotFound();
        }

        vm.LastReport = UploadReportTempData.Read(TempData);

        return ShowBoard(vm);
    }

    [HttpPost(UploadPath)]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(ImageUploadLimits.MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageUploadLimits.MaxRequestBytes)]
    public async Task<IActionResult> Upload([FromRoute] int relicId, [FromForm] RelicImageUploadVM form)
    {
        var board = await _imageService.GetBoardAsync(relicId);
        if (board is null)
        {
            return AdminNotFound();
        }

        var report = await _imageService.UploadBatchAsync(relicId, BuildUploads(form));

        if (!string.IsNullOrEmpty(report.Error))
        {
            TempData["AdminError"] = report.Error;
        }
        else if (report.SavedCount > 0 && report.HasRejections)
        {
            TempData["AdminWarning"] =
                $"Đã thêm {report.SavedCount} ảnh. {report.Rejections.Count} tệp bị từ chối.";
            AddRejectionLink();
        }
        else if (report.SavedCount > 0)
        {
            TempData["AdminSuccess"] = $"Đã thêm {report.SavedCount} ảnh cho di tích {board.RelicName}.";
        }
        else if (report.HasRejections)
        {
            TempData["AdminError"] = $"Không tệp nào được lưu. {report.Rejections.Count} tệp bị từ chối.";
            AddRejectionLink();
        }

        UploadReportTempData.Save(TempData, report);

        return Redirect(BoardUrl(relicId));
    }

    [HttpPost(DetailPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDetail(
        [FromRoute] int relicId,
        [FromRoute] int imageId,
        [FromForm] string? caption,
        [FromForm] string? imageSource)
    {
        var result = await _imageService.UpdateDetailAsync(relicId, imageId, caption, imageSource);
        if (result.Success)
        {
            TempData["AdminSuccess"] = "Đã lưu chú thích và nguồn của ảnh.";

            return Redirect(BoardUrl(relicId));
        }

        var vm = await BuildBoardAsync(relicId);
        if (vm is null)
        {
            return AdminNotFound();
        }

        if (vm.Board.Images.All(i => i.Id != imageId))
        {
            TempData["AdminError"] = result.Error;

            return Redirect(BoardUrl(relicId));
        }

        vm.EditingImageId = imageId;
        vm.EditCaption = caption;
        vm.EditSource = imageSource;

        if (result.Field == ImageDetailValidator.CaptionField)
        {
            vm.EditCaptionError = result.Error;
        }
        else
        {
            vm.EditSourceError = result.Error;
        }

        return ShowBoard(vm);
    }

    [HttpPost(ThumbnailPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetThumbnail([FromRoute] int relicId, [FromRoute] int imageId)
    {
        var result = await _imageService.SetThumbnailAsync(relicId, imageId);
        if (result.Success)
        {
            TempData["AdminSuccess"] = "Đã đổi ảnh đại diện của di tích.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(BoardUrl(relicId));
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete([FromRoute] int relicId, [FromRoute] int imageId)
    {
        var vm = await BuildBoardAsync(relicId);
        if (vm is null)
        {
            return AdminNotFound();
        }

        var image = vm.Board.Images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Xóa ảnh di tích";
        ViewData["AdminCrumbs"] = BuildCrumbs(vm.Board.RelicName, vm.Board.RelicId, "Xóa ảnh");

        return View("Delete", new RelicImageDeleteVM
        {
            RelicId = vm.Board.RelicId,
            RelicName = vm.Board.RelicName,
            Image = image,
            BoardUrl = BoardUrl(relicId)
        });
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int relicId, [FromRoute] int imageId)
    {
        var result = await _imageService.DeleteAsync(relicId, imageId);
        if (result.Success)
        {
            TempData["AdminSuccess"] = $"Đã xóa {result.Data}.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(BoardUrl(relicId));
    }

    private IActionResult ShowBoard(RelicImageBoardVM vm)
    {
        ViewData["Title"] = $"Ảnh của {vm.Board.RelicName}";
        ViewData["AdminCrumbs"] = BuildCrumbs(vm.Board.RelicName, vm.Board.RelicId, "Ảnh");

        return View("Index", vm);
    }

    private static List<AdminCrumb> BuildCrumbs(string relicName, int relicId, string leaf) => new()
    {
        new AdminCrumb { Label = "Quản trị", Url = DashboardController.IndexPath },
        new AdminCrumb { Label = "Di tích", Url = RelicAdminController.ListPath },
        new AdminCrumb { Label = relicName, Url = $"/admin/di-tich/sua/{relicId}" },
        new AdminCrumb { Label = leaf }
    };

    private async Task<RelicImageBoardVM?> BuildBoardAsync(int relicId)
    {
        var board = await _imageService.GetBoardAsync(relicId);
        if (board is null)
        {
            return null;
        }

        var publicUrl = SiteUrls.OnPort(Request, _adminSite.PublicPort, $"/di-tich/{board.RelicSlug}");

        return RelicImageBoardFactory.ToBoardVM(board, RelicAdminController.ListPath, publicUrl);
    }

    private static string BoardUrl(int relicId) => $"/admin/di-tich/{relicId}/anh";

    private void AddRejectionLink()
    {
        TempData["AdminAlertLinkUrl"] = "#upload-rejects";
        TempData["AdminAlertLinkText"] = "Xem chi tiết";
    }

    private static List<RelicImageUpload> BuildUploads(RelicImageUploadVM form)
    {
        var uploads = new List<RelicImageUpload>();
        if (form.Files is null)
        {
            return uploads;
        }

        for (var index = 0; index < form.Files.Count; index++)
        {
            var file = form.Files[index];
            if (file is null || file.Length == 0 && string.IsNullOrEmpty(file.FileName))
            {
                continue;
            }

            uploads.Add(new RelicImageUpload
            {
                File = file,
                Caption = Pick(form.Captions, index) ?? form.SharedCaption,
                Source = Pick(form.Sources, index) ?? form.SharedSource
            });
        }

        return uploads;
    }

    private static string? Pick(List<string>? values, int index)
    {
        if (values is null || index >= values.Count)
        {
            return null;
        }

        var value = values[index];

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
