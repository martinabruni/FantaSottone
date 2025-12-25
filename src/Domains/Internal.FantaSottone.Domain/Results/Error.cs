namespace Internal.FantaSottone.Domain.Results;

public sealed class Error
{
    public string Message { get; set; } = string.Empty;
    public string? Code { get; set; }

    public Error(string message, string? code = null)
    {
        Message = message;
        Code = code;
    }
}
