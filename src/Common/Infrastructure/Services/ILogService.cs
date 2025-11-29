using Common.DTOs;
using Common.Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace Common.Infrastructure.Services;

public interface ILogService
{
    Task LogAsync(
        string action,
        string? entityType = null,
        long? entityId = null,
        string? details = null,
        long? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);

    Task LogFromHttpContextAsync(
        HttpContext httpContext,
        string action,
        string? entityType = null,
        long? entityId = null,
        string? details = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<Log>> GetLogsAsync(
        int perInPage,
        int page,
        CancellationToken cancellationToken = default);
}