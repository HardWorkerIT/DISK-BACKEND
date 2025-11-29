using Common.Abstracts;
using Common.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Users.Application.DTOs.Responses;

[SwaggerSchema(Title = "GetUserResponse", Description = "Ответ при получении списка пользователей")]
public class GetUserResponse(IEnumerable<UserDTOWithoutPassword> objects) : IResponse
{
    public IEnumerable<UserDTOWithoutPassword> Objects { get; set; } = objects;
}