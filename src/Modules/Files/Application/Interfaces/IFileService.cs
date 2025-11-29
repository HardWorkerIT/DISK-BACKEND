using Common.DTOs;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Files.Domain.Entities.File;

namespace Files.Application.Interfaces;

public interface IFileService
{
    Task<File?> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<File>> SelectAsync(GetFileRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateFileRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(DeleteFileRequest request, CancellationToken cancellationToken);
}
