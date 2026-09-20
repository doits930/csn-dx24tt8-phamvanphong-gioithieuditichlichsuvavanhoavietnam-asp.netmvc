using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DiTichVietNam.Web.Services.Common;

public static class DatabaseConflict
{
    private const int UniqueConstraintCode = 2067;
    private const int PrimaryKeyConstraintCode = 1555;

    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqliteException sqlite
        && (sqlite.SqliteExtendedErrorCode == UniqueConstraintCode
            || sqlite.SqliteExtendedErrorCode == PrimaryKeyConstraintCode);
}
