using Files.Application.DTOs.Base;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.Mappers;

public static class FolderMapper
{
    public static Folder GetModel(FolderDTO body)
    {
        return new Folder()
        {
            Id = body.Id,
            Name = body.Name ?? string.Empty,
            ParentId = body.ParentId,
            DeletedAt = null
        };
    }

    public static Folder GetModel(CreateFolderRequest request)
    {
        return new Folder()
        {
            Id = 0,
            Name = request.Name ?? string.Empty,
            ParentId = request.ParentId,
            DeletedAt = null
        };
    }

    public static Folder GetModel(UpdateFolderRequest request)
    {
        return new Folder()
        {
            Id = request.Id,
            Name = request.Name ?? string.Empty,
            ParentId = request.ParentId,
            DeletedAt = null
        };
    }

    public static FolderDTO GetBody(Folder model)
    {
        return new FolderDTO()
        {
            Id = model.Id,
            Name = model.Name,
            ParentId = model.ParentId,
        };
    }
}
