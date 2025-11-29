using Common.Abstracts;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace App.Application.DTOs.Requests;

[SwaggerSchema(Title = "GetLogsRequest", Description = "Запрос для получения списка логов с пагинацией")]
public class GetLogsRequest(int perInPage, int page) : BasePerInPageRequest
{
    [Required(ErrorMessage = "PerInPage обязателен")]
    [Range(1, 100, ErrorMessage = "PerInPage должен быть от 1 до 100")]
    [SwaggerSchema(Description = "Количество записей на странице")]
    public new int PerInPage { get; set; } = perInPage;

    [Required(ErrorMessage = "Page обязателен")]
    [Range(1, int.MaxValue, ErrorMessage = "Page должен быть больше или равен 1")]
    [SwaggerSchema(Description = "Номер страницы (начиная с 1)")]
    public new int Page { get; set; } = page;
}