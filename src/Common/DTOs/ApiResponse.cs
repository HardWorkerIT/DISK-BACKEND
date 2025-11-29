namespace Common.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }

    public ApiResponse(bool success, T? data = default, string? message = null, string? error = null, Dictionary<string, string[]>? validationErrors = null)
    {
        Success = success;
        Data = data;
        Message = message;
        Error = error;
        ValidationErrors = validationErrors;
    }

    public static ApiResponse<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponse<T>(true, data, message);
    }

    public static ApiResponse<T> ErrorResult(string error, string? message = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse<T>(false, default, message, error, validationErrors);
    }
}

public class ApiResponse : ApiResponse<object>
{
    public ApiResponse(bool success, object? data = default, string? message = null, string? error = null, Dictionary<string, string[]>? validationErrors = null)
        : base(success, data, message, error, validationErrors)
    {
    }

    public static new ApiResponse SuccessResult(object? data = null, string? message = null)
    {
        return new ApiResponse(true, data, message);
    }

    public static new ApiResponse ErrorResult(string error, string? message = null, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResponse(false, null, message, error, validationErrors);
    }
}