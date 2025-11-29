using Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Common.Abstracts;

public abstract class BaseController : ControllerBase
{
    protected ActionResult<ApiResponse<T>> Success<T>(T data, string? message = null)
    {
        return Ok(ApiResponse<T>.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse<T>> Error<T>(string error, string? message = null)
    {
        return BadRequest(ApiResponse<T>.ErrorResult(error, message));
    }

    protected ActionResult<ApiResponse> Success(object? data = null, string? message = null)
    {
        return Ok(ApiResponse.SuccessResult(data, message));
    }

    protected ActionResult<ApiResponse> Error(string error, string? message = null)
    {
        return BadRequest(ApiResponse.ErrorResult(error, message));
    }

    protected ActionResult<ApiResponse<T>> NotFound<T>(string message = "Ресурс не найден")
    {
        return NotFound(ApiResponse<T>.ErrorResult("NOT_FOUND", message));
    }

    protected ActionResult<ApiResponse> NotFound(string message = "Ресурс не найден")
    {
        return NotFound(ApiResponse.ErrorResult("NOT_FOUND", message));
    }

    protected ActionResult<ApiResponse<T>> Unauthorized<T>(string message = "Не авторизован")
    {
        return Unauthorized(ApiResponse<T>.ErrorResult("UNAUTHORIZED", message));
    }

    protected ActionResult<ApiResponse> Unauthorized(string message = "Не авторизован")
    {
        return Unauthorized(ApiResponse.ErrorResult("UNAUTHORIZED", message));
    }

    protected ActionResult<ApiResponse<T>> ValidationError<T>(Dictionary<string, string[]> errors)
    {
        return BadRequest(ApiResponse<T>.ErrorResult("VALIDATION_ERROR", "Ошибки валидации", errors));
    }

    protected ActionResult<ApiResponse> ValidationError(Dictionary<string, string[]> errors)
    {
        return BadRequest(ApiResponse.ErrorResult("VALIDATION_ERROR", "Ошибки валидации", errors));
    }

    protected bool ValidateRequest<TRequest, TResponse>(TRequest request, out ActionResult<ApiResponse<TResponse>>? errorResult) where TRequest : BaseRequest
    {
        errorResult = null;

        if (!request.IsValid(out var validationResults))
        {
            var errors = new Dictionary<string, List<string>>();
            var namingPolicy = JsonNamingPolicy.SnakeCaseLower;

            foreach (var result in validationResults)
            {
                var fieldName = result.MemberNames.FirstOrDefault() ?? "General";
                // Преобразуем имя поля в snake_case
                var snakeCaseFieldName = namingPolicy.ConvertName(fieldName);

                if (!errors.ContainsKey(snakeCaseFieldName))
                    errors[snakeCaseFieldName] = new List<string>();

                errors[snakeCaseFieldName].Add(result.ErrorMessage ?? "Ошибка валидации");
            }

            var stringArrayErrors = errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToArray()
            );
            errorResult = ValidationError<TResponse>(stringArrayErrors);
            return false;
        }

        return true;
    }

}