using Common.Attributes;
using Common.Constants;
using Common.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.Infrastructure.Middleware;

/// <summary>
/// Middleware для проверки авторизации пользователя
/// Проверяет наличие валидной cookie аутентификации
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Проверяем, требуется ли авторизация для данного пути
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var requireAuth = endpoint?.Metadata.GetMetadata<RequireAuthAttribute>() != null;

        if (requireAuth)
        {
            // Пытаемся аутентифицировать пользователя
            // Используем строковую константу вместо CookieAuthenticationDefaults для избежания зависимости
            var authResult = await context.AuthenticateAsync("Cookies");

            if (!authResult.Succeeded || authResult.Principal == null)
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

            // Проверяем наличие claim "User"
            var userClaim = authResult.Principal.FindFirst("User");
            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                _logger.LogWarning("У пользователя отсутствует claim 'User': {Path}", context.Request.Path);

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = ApiResponse.ErrorResult("UNAUTHORIZED", "Невалидная сессия пользователя");
                var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                await context.Response.WriteAsync(json);
                return;
            }

            // Устанавливаем пользователя в контекст для дальнейшего использования
            // Примечание: RoleId должен быть добавлен в claims при аутентификации в CAccess
            context.User = authResult.Principal;
        }

        await _next(context);
    }
}

