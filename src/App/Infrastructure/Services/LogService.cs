using Common.Infrastructure.Data;
using Common.Infrastructure.Services;
using Access.Application.Interfaces;
using Common.Abstracts;
using Common.Infrastructure.Repositories;
using Common.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Common.Infrastructure.Abstracts;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly IGenericRepository<Log> _repository;
    private readonly IDbContext _context;
    private readonly IAccessService _accessService;

    public LogService(IDbContext context, IAccessService accessService)
    {
        _context = context;
        _accessService = accessService;
        _repository = new GenericRepository<Log>(context);
    }

    public async Task LogAsync(
        string action,
        string? entityType = null,
        long? entityId = null,
        string? details = null,
        long? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var log = new Log
        {
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            UserId = userId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.CreateAsync(log, cancellationToken);
    }

    public async Task LogFromHttpContextAsync(
        HttpContext httpContext,
        string action,
        string? entityType = null,
        long? entityId = null,
        string? details = null,
        CancellationToken cancellationToken = default)
    {
        long? userId = null;
        string? ipAddress = null;
        string? userAgent = null;

        // Получаем IP адрес
        ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

        // Получаем User-Agent
        if (httpContext.Request.Headers.TryGetValue("User-Agent", out var userAgentValue))
        {
            userAgent = userAgentValue.ToString();
        }

        // Получаем UserId из claims через AccessService
        try
        {
            var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (authResult.Succeeded && authResult.Principal != null)
            {
                var userClaim = authResult.Principal.FindFirst("User");
                if (userClaim != null && !string.IsNullOrEmpty(userClaim.Value))
                {
                    try
                    {
                        var userDTO = await _accessService.IdentifyAsync(userClaim.Value, cancellationToken);
                        userId = userDTO.Id;
                    }
                    catch
                    {
                        // Если не удалось получить пользователя, оставляем userId = null
                    }
                }
            }
            else
            {
                // Пробуем получить из HttpContext.User напрямую
                var userClaimFromContext = httpContext.User?.FindFirst("User");
                if (userClaimFromContext != null && !string.IsNullOrEmpty(userClaimFromContext.Value))
                {
                    try
                    {
                        var userDTO = await _accessService.IdentifyAsync(userClaimFromContext.Value, cancellationToken);
                        userId = userDTO.Id;
                    }
                    catch
                    {
                        // Если не удалось получить пользователя, оставляем userId = null
                    }
                }
            }
        }
        catch
        {
            // Если произошла ошибка при получении UserId, продолжаем без него
        }

        await LogAsync(action, entityType, entityId, details, userId, ipAddress, userAgent, cancellationToken);
    }

    public async Task<PagedResponse<Common.Infrastructure.Data.Log>> GetLogsAsync(
        int perInPage,
        int page,
        CancellationToken cancellationToken = default)
    {
        // Используем прямой доступ к DbSet для эффективной работы с большими объемами данных
        var dbContext = (Microsoft.EntityFrameworkCore.DbContext)_context;
        var dbSet = dbContext.Set<Common.Infrastructure.Data.Log>();

        var query = dbSet.Where(x => x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * perInPage)
            .Take(perInPage)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Common.Infrastructure.Data.Log>(items, totalCount, page, perInPage);
    }
}
