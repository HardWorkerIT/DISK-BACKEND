using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Requests;

[SwaggerSchema(Title = "UpdateFolderRequest", Description = "Запрос для обновления существующей папки")]
public class UpdateFolderRequest : BaseRequest
{
    [Required(ErrorMessage = "Id обязателен")]
    [Range(1, long.MaxValue, ErrorMessage = "Id должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор папки")]
    public long Id { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "ParentId должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор папки (для вложенных папок)")]
    public long? ParentId { get; set; }

    [Required(ErrorMessage = "Name обязателен")]
    [StringLength(255, ErrorMessage = "Name не должен превышать 255 символов")]
    [SwaggerSchema(Description = "Название папки")]
    public string? Name { get; set; }
}
