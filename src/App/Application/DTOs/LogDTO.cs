using Swashbuckle.AspNetCore.Annotations;

namespace App.Application.DTOs;

[SwaggerSchema(Description = "Данные лога")]
public class LogDTO
{
    [SwaggerSchema(Description = "Идентификатор лога")]
    public long Id { get; set; }

    [SwaggerSchema(Description = "Идентификатор пользователя")]
    public long? UserId { get; set; }

    [SwaggerSchema(Description = "IP адрес")]
    public string? IpAddress { get; set; }

    [SwaggerSchema(Description = "User-Agent")]
    public string? UserAgent { get; set; }

    [SwaggerSchema(Description = "Действие")]
    public string Action { get; set; } = string.Empty;

    [SwaggerSchema(Description = "Тип сущности")]
    public string? EntityType { get; set; }

    [SwaggerSchema(Description = "Идентификатор сущности")]
    public long? EntityId { get; set; }

    [SwaggerSchema(Description = "Детали")]
    public string? Details { get; set; }

    [SwaggerSchema(Description = "Дата и время создания")]
    public DateTimeOffset CreatedAt { get; set; }
}