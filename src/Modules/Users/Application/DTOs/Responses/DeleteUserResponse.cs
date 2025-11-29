using Common.Abstracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.Responses;

[SwaggerSchema(Title = "DeleteUserResponse", Description = "Ответ при удалении пользователя")]
public class DeleteUserResponse : IResponse
{
}