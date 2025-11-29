using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Requests;

[SwaggerSchema(Title = "CreateFileRequest", Description = "Запрос для создания нового файла")]
public class CreateFileRequest : BaseRequest
{
    [Required(ErrorMessage = "FolderId обязателен")]
    [Range(1, long.MaxValue, ErrorMessage = "FolderId должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор папки с файлами")]
    public long FolderId { get; set; }

    [Required(ErrorMessage = "Name обязателен")]
    [StringLength(255, ErrorMessage = "Name не должен превышать 255 символов")]
    [SwaggerSchema(Description = "Название файла")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "CreatedAt обязателен")]
    [SwaggerSchema(Description = "Дата и время создания файла")]
    public DateTimeOffset CreatedAt { get; set; }

    [Required(ErrorMessage = "UpdatedAt обязателен")]
    [SwaggerSchema(Description = "Дата и время обновления файла")]
    public DateTimeOffset UpdatedAt { get; set; }
}