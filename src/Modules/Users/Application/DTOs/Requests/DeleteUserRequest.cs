using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.Requests;

[SwaggerSchema(Title = "DeleteUserRequest", Description = "Запрос для удаления пользователя")]
public class DeleteUserRequest(long id) : BaseRequest
{
    [Required(ErrorMessage = "Id обязателен")]
    [Range(1, long.MaxValue, ErrorMessage = "Id должен быть больше 0")]
    [SwaggerSchema(Description = "Идентификатор пользователя для удаления")]
    public long Id { get; set; } = id;
}