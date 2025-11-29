using Files.Application.DTOs.Base;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Files.Domain.Entities.File;

namespace Files.Application.Mappers;

public static class FileMapper
{
    public static File GetModel(FileDTO body)
    {
        return new File()
        {
            Id = body.Id,
            FolderId = body.FolderId,
            Name = body.Name ?? string.Empty,
            CreatedAt = body.CreatedAt,
            UpdatedAt = body.UpdatedAt,
            DeletedAt = null
        };
    }

    public static File GetModel(CreateFileRequest request)
    {
        return new File()
        {
            Id = 0,
            FolderId = request.FolderId,
            Name = request.Name ?? string.Empty,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            DeletedAt = null
        };
    }

    public static File GetModel(UpdateFileRequest request)
    {
        return new File()
        {
            Id = 0,
            FolderId = request.FolderId,
            Name = request.Name ?? string.Empty,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt,
            DeletedAt = null
        };
    }

    public static FileDTO GetBody(File model)
    {
        return new FileDTO()
        {
            Id = model.Id,
            FolderId = model.FolderId,
            Name = model.Name ?? string.Empty,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
        };
    }
}
