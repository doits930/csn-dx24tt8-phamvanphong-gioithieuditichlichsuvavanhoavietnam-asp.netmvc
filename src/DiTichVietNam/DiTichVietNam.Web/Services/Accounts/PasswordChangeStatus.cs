namespace DiTichVietNam.Web.Services.Accounts;

public enum PasswordChangeStatus
{
    Changed = 1,
    WrongCurrentPassword = 2,
    LockedOut = 3,
    Rejected = 4
}
