using Common.Abstracts;
using Files.Application.DTOs.Base;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Responses;

[SwaggerSchema(Title = "GetFileResponse", Description = "Ответ при получении списка файлов")]
public class GetFileResponse(IEnumerable<FileDTO> objects) : IResponse
{
    public IEnumerable<FileDTO> Objects { get; set; } = objects;
}
