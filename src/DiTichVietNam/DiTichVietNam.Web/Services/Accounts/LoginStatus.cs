namespace DiTichVietNam.Web.Services.Accounts;

public enum LoginStatus
{
    Success = 1,
    InvalidCredentials = 2,
    LockedOut = 3,
    Forbidden = 4
}
