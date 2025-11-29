using Common.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Responses;

public class CreateFolderResponse(long? parentId, string? name) : IResponse
{
    public long? ParentId { get; set; } = parentId;
    public string? Name { get; set; } = name;
}
