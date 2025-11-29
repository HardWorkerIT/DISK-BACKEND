using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs.Requests;

[SwaggerSchema(Title = "UpdateUserRequest", Description = "Запрос для обновления существующего пользователя")]
public class UpdateUserRequest : BaseRequest
{
    [Required(ErrorMessage = "Id обязателен")]
    [Range(1, long.MaxValue, ErrorMessage = "Id должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор пользователя")]
    public long Id { get; set; }

    [Required(ErrorMessage = "Login обязателен")]
    [SwaggerSchema(Description = "Логин пользователя")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Email обязателен")]
    [SwaggerSchema(Description = "Email пользователя")]
    public string? Email { get; set; }

    [SwaggerSchema(Description = "Имя пользователя")]
    public string? FirstName { get; set; }

    [SwaggerSchema(Description = "Отчество пользователя")]
    public string? MiddleName { get; set; }

    [SwaggerSchema(Description = "Фамилия пользователя")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "RoleId обязателен")]
    [Range(1, 3, ErrorMessage = "RoleId должен быть от 1 до 3")]
    [SwaggerSchema(Description = "Идентификатор роли пользователя")]
    public int RoleId { get; set; }
}