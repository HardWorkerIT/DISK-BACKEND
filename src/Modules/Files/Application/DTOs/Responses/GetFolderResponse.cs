using Common.Abstracts;
using Files.Application.DTOs.Base;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Responses;

[SwaggerSchema(Title = "GetFolderResponse", Description = "Ответ при получении папок")]
public class GetFolderResponse(IEnumerable<FolderDTO> objects) : IResponse
{
    public IEnumerable<FolderDTO> Objects { get; set; } = objects;
}
