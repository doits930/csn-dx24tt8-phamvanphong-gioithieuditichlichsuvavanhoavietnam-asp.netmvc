using DiTichVietNam.Web.Services.Images;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class RelicImageBoardFactory
{
    public static RelicImageBoardVM ToBoardVM(RelicImageBoard board, string listUrl, string publicUrl) => new()
    {
        Board = board,
        ListUrl = listUrl,
        UploadUrl = $"/admin/di-tich/{board.RelicId}/anh/tai-len",
        PublicUrl = publicUrl
    };
}
