using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для проверки прав доступа на основе ролей пользователя
/// Проверяет наличие атрибута [RequireRole] и валидность роли пользователя
/// </summary>
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Проверяем, требуется ли проверка ролей для данного пути
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var requireRole = endpoint?.Metadata.GetMetadata<RequireRoleAttribute>();

        if (requireRole != null && requireRole.AllowedRoleIds.Length > 0)
        {
            // Проверяем, что пользователь аутентифицирован
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Попытка доступа к защищенному ресурсу без аутентификации: {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.ErrorResult("UNAUTHORIZED", "Требуется авторизация");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                await context.Response.WriteAsync(json);
                return;
            }

            // Получаем RoleId из claims
            var roleIdClaim = context.User.FindFirst("RoleId");

            if (roleIdClaim == null || !int.TryParse(roleIdClaim.Value, out var userRoleId))
            {
                _logger.LogWarning("У пользователя отсутствует claim 'RoleId' или он невалиден: {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.ErrorResult("FORBIDDEN", "Недостаточно прав доступа");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                await context.Response.WriteAsync(json);
                return;
            }

            // Проверяем, есть ли у пользователя одна из разрешенных ролей
            var hasAccess = requireRole.AllowedRoleIds.Contains(userRoleId);

            if (!hasAccess)
            {
                _logger.LogWarning(
                    "Пользователь с ролью {UserRole} пытается получить доступ к ресурсу, требующему ролей {RequiredRoles}: {Path}",
                    userRoleId,
                    string.Join(", ", requireRole.AllowedRoleIds),
                    context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.ErrorResult("FORBIDDEN", "Недостаточно прав доступа");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                await context.Response.WriteAsync(json);
                return;
            }
        }

        await _next(context);
    }
}
