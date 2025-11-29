using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Requests;

[SwaggerSchema(Title = "IdentifyRequest", Description = "Запрос для идентификации пользователя")]
public class IdentifyRequest(string cookie) : BaseRequest
{
    [Required(ErrorMessage = "Cookie обязательна")]
    [StringLength(255, ErrorMessage = "Cookie не должен превышать 255 символов")]
    [SwaggerSchema(Description = "Cookie пользователя")]
    public string Cookie { get; set; } = cookie;
}