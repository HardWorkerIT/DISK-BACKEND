using Common.Abstracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.DTOs.Responses;

public class CreateFileResponse(long folderId, string? name, DateTimeOffset createdAt, DateTimeOffset updatedAt) : IResponse
{
    public long FolderId { get; set; } = folderId;
    public string? Name { get; set; } = name;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public DateTimeOffset UpdatedAt { get; set; } = updatedAt;
}
