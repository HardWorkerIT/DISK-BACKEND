using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Access.Application.DTOs.Responses;

[SwaggerSchema(Title = "AuthentificateResponse", Description = "Ответ при аутентификации пользователя")]
public class AuthentificateResponse() : IResponse
{
}