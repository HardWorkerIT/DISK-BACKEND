using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs.Requests;

[SwaggerSchema(Title = "CreateUserRequest", Description = "Запрос для создания нового пользователя")]
public class CreateUserRequest : BaseRequest
{
    [Required(ErrorMessage = "Login обязателен")]
    [SwaggerSchema(Description = "Логин пользователя")]
    public string? Login { get; set; }

    [Required(ErrorMessage = "Password обязателен")]
    [SwaggerSchema(Description = "Пароль пользователя")]
    public string? Password { get; set; }

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