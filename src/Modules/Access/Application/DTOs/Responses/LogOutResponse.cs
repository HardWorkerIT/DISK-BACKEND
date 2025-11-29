using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Responses;

[SwaggerSchema(Title = "LogOutResponse", Description = "Ответ при выходе пользователя")]
public class LogOutResponse : IResponse
{
}