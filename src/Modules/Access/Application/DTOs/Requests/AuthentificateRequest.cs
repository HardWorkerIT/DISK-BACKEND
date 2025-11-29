using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Requests;

[SwaggerSchema(Title = "AuthentificateRequest", Description = "Запрос для аутентификации пользователя")]
public class AuthentificateRequest(string? login, string? password) : BaseRequest
{
    [Required(ErrorMessage = "Login обязателен")]
    [StringLength(255, ErrorMessage = "Login не должен превышать 255 символов")]
    [SwaggerSchema(Description = "Логин пользователя")]
    public string? Login { get; set; } = login;

    [Required(ErrorMessage = "Password обязателен")]
    [StringLength(255, ErrorMessage = "Password не должен превышать 255 символов")]
    [SwaggerSchema(Description = "Пароль пользователя")]
    public string? Password { get; set; } = password;
}