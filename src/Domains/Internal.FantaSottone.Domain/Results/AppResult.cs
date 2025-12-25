namespace Internal.FantaSottone.Domain.Results;

public class AppResult
{
    public List<Error> Errors { get; set; } = [];
    public AppStatusCode StatusCode { get; set; }

    public bool IsSuccess => StatusCode is AppStatusCode.Ok or AppStatusCode.Created;
    public bool IsFailure => !IsSuccess;

    // Static factory methods
    public static AppResult Success() => new() { StatusCode = AppStatusCode.Ok };

    public static AppResult BadRequest(string message, string? code = null) => new()
    {
        StatusCode = AppStatusCode.BadRequest,
        Errors = [new Error(message, code)]
    };

    public static AppResult BadRequest(List<Error> errors) => new()
    {
        StatusCode = AppStatusCode.BadRequest,
        Errors = errors
    };

    public static AppResult Unauthorized(string message = "Unauthorized") => new()
    {
        StatusCode = AppStatusCode.Unauthorized,
        Errors = [new Error(message, "UNAUTHORIZED")]
    };

    public static AppResult Forbidden(string message = "Forbidden") => new()
    {
        StatusCode = AppStatusCode.Forbidden,
        Errors = [new Error(message, "FORBIDDEN")]
    };

    public static AppResult NotFound(string message = "Resource not found") => new()
    {
        StatusCode = AppStatusCode.NotFound,
        Errors = [new Error(message, "NOT_FOUND")]
    };

    public static AppResult Conflict(string message, string? code = null) => new()
    {
        StatusCode = AppStatusCode.Conflict,
        Errors = [new Error(message, code ?? "CONFLICT")]
    };

    public static AppResult InternalServerError(string message = "An internal error occurred") => new()
    {
        StatusCode = AppStatusCode.InternalServerError,
        Errors = [new Error(message, "INTERNAL_ERROR")]
    };
}

public sealed class AppResult<T> : AppResult //where T : class
{
    public T? Value { get; set; }

    // Static factory methods for AppResult<T>
    public static AppResult<T> Success(T data) => new()
    {
        StatusCode = AppStatusCode.Ok,
        Value = data
    };

    public static AppResult<T> Created(T data) => new()
    {
        StatusCode = AppStatusCode.Created,
        Value = data
    };

    public new static AppResult<T> BadRequest(string message, string? code = null) => new()
    {
        StatusCode = AppStatusCode.BadRequest,
        Errors = [new Error(message, code)]
    };

    public new static AppResult<T> BadRequest(List<Error> errors) => new()
    {
        StatusCode = AppStatusCode.BadRequest,
        Errors = errors
    };

    public new static AppResult<T> Unauthorized(string message = "Unauthorized") => new()
    {
        StatusCode = AppStatusCode.Unauthorized,
        Errors = [new Error(message, "UNAUTHORIZED")]
    };

    public new static AppResult<T> Forbidden(string message = "Forbidden") => new()
    {
        StatusCode = AppStatusCode.Forbidden,
        Errors = [new Error(message, "FORBIDDEN")]
    };

    public new static AppResult<T> NotFound(string message = "Resource not found") => new()
    {
        StatusCode = AppStatusCode.NotFound,
        Errors = [new Error(message, "NOT_FOUND")]
    };

    public new static AppResult<T> Conflict(string message, string? code = null) => new()
    {
        StatusCode = AppStatusCode.Conflict,
        Errors = [new Error(message, code ?? "CONFLICT")]
    };

    public new static AppResult<T> InternalServerError(string message = "An internal error occurred") => new()
    {
        StatusCode = AppStatusCode.InternalServerError,
        Errors = [new Error(message, "INTERNAL_ERROR")]
    };
}
