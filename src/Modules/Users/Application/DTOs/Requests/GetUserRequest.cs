using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.Requests;

[SwaggerSchema(Title = "GetUserRequest", Description = "Запрос для получения списка пользователей с пагинацией")]
public class GetUserRequest(int perInPage, int page) : BasePerInPageRequest
{
    [Required(ErrorMessage = "PerInPage обязателен")]
    [Range(1, 100, ErrorMessage = "PerInPage должен быть от 1 до 100")]
    [SwaggerSchema(Description = "Количество записей на странице")]
    public new int PerInPage { get; set; } = perInPage;

    [Required(ErrorMessage = "Page обязателен")]
    [Range(0, int.MaxValue, ErrorMessage = "Page должен быть больше или равен 0")]
    [SwaggerSchema(Description = "Номер страницы (начиная с 0)")]
    public new int Page { get; set; } = page;
}