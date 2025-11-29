using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Requests;

[SwaggerSchema(Title = "GetStandardRequest", Description = "Запрос для получения списка файлов по папке с пагинацией")]
public class GetFileRequest(long folderId, int perInPage, int page) : BasePerInPageRequest
{
    [Required(ErrorMessage = "FolderId обязателен")]
    [Range(1, long.MaxValue, ErrorMessage = "FolderId должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор папки с файлами")]
    public long FolderId { get; set; } = folderId;

    [Required(ErrorMessage = "PerInPage обязателен")]
    [Range(1, 100, ErrorMessage = "PerInPage должен быть от 1 до 100")]
    [SwaggerSchema(Description = "Количество записей на странице")]
    public new int PerInPage { get; set; } = perInPage;

    [Required(ErrorMessage = "Page обязателен")]
    [Range(1, int.MaxValue, ErrorMessage = "Page должен быть больше или равен 1")]
    [SwaggerSchema(Description = "Номер страницы (начиная с 1)")]
    public new int Page { get; set; } = page;
}
