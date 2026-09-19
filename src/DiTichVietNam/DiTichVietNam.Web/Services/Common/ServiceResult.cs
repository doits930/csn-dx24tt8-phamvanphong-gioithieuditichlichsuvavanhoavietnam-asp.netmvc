namespace DiTichVietNam.Web.Services.Common;

public class ServiceResult
{
    public bool Success { get; protected set; }
    public string? Error { get; protected set; }
    public string? Field { get; protected set; }

    public static ServiceResult Ok() => new() { Success = true };

    public static ServiceResult Fail(string error, string? field = null)
        => new() { Success = false, Error = error, Field = field };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Ok(T data) => new() { Success = true, Data = data };

    public static new ServiceResult<T> Fail(string error, string? field = null)
        => new() { Success = false, Error = error, Field = field };
}
