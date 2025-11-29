using Common.Infrastructure.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Common.Extensions;

/// <summary>
/// Расширения для регистрации middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Добавляет middleware для проверки авторизации
    /// </summary>
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }

    /// <summary>
    /// Добавляет middleware для проверки прав доступа на основе ролей
    /// </summary>
    public static IApplicationBuilder UseAuthorizationMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}