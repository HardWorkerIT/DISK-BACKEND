using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Requests;

[SwaggerSchema(Title = "GetFolderRequest", Description = "Запрос для получения списка папок с пагинацией")]
public class GetFolderRequest(int perInPage, int page, long? parentId = null, string? search = null, string sortBy = "id", string sortDirection = "asc") : BasePerInPageRequest
{
    [Required(ErrorMessage = "PerInPage обязателен")]
    [Range(1, 100, ErrorMessage = "PerInPage должен быть от 1 до 100")]
    [SwaggerSchema(Description = "Количество записей на странице")]
    public new int PerInPage { get; set; } = perInPage;

    [Required(ErrorMessage = "Page обязателен")]
    [Range(1, 1000000, ErrorMessage = "Page должен быть от 1 до 1000000")]
    [SwaggerSchema(Description = "Номер страницы (начиная с 0)")]
    public new int Page { get; set; } = page;

    [Range(1, long.MaxValue, ErrorMessage = "ParentId должен быть больше 0")]
    public long? ParentId { get; set; } = parentId;

    [StringLength(100, ErrorMessage = "Поисковая строка не должна превышать 100 символов")]
    public string? Search { get; set; } = search;

    [RegularExpression("^(id|name)$", ErrorMessage = "SortBy должен быть id или name")]
    public string SortBy { get; set; } = sortBy;

    [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDirection должен быть asc или desc")]
    public string SortDirection { get; set; } = sortDirection;
}
