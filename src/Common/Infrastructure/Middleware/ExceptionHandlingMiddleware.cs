using Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Diagnostics;

namespace Common.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(correlationId)
            || correlationId.Length > 128
            || correlationId.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_'))
        {
            correlationId = context.TraceIdentifier;
        }

        context.Response.Headers["X-Correlation-ID"] = correlationId;
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        context.Response.Headers["X-Trace-ID"] = traceId;
        using var logScope = logger.BeginScope(new Dictionary<string, object>
        {
            ["correlation_id"] = correlationId,
            ["trace_id"] = traceId,
            ["http_method"] = context.Request.Method
        });
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Request cancelled by client: {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        catch (OperationCanceledException exception)
        {
            logger.LogWarning(exception, "Request timed out: {Method} {Path}", context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            context.Response.Headers["X-Trace-ID"] = traceId;
            var response = ApiResponse.ErrorResult("REQUEST_TIMEOUT", "Время ожидания запроса истекло");
            await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions, context.RequestAborted);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception {CorrelationId} for {Method} {Path}", correlationId, context.Request.Method, context.Request.Path);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            context.Response.Headers["X-Trace-ID"] = traceId;
            var response = ApiResponse.ErrorResult("INTERNAL_SERVER_ERROR", $"Внутренняя ошибка сервера. Идентификатор: {correlationId}");
            await JsonSerializer.SerializeAsync(context.Response.Body, response, JsonOptions, context.RequestAborted);
        }
    }
}
