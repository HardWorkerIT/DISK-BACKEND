using Common.Abstracts;
using Common.DTOs;
using Common.Infrastructure.Abstracts;
using Common.Infrastructure.Repositories;
using Files.Application.DTOs.Requests;
using Files.Application.Interfaces;
using Files.Application.Mappers;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Files.Domain.Entities.File;

namespace Files.Application.Services;

public sealed class FileService(IDbContext context) : IFileService
{
    public readonly IGenericRepository<File> _repository = new GenericRepository<File>(context);

    public Task<File?> CreateAsync(CreateFileRequest request, CancellationToken cancellationToken)
    {
        return _repository.CreateAsync(FileMapper.GetModel(request), cancellationToken);
    }

    public async Task DeleteAsync(DeleteFileRequest request, CancellationToken cancellationToken)
    {
        File? file = await _repository.FindByIdAsync(request.Id, cancellationToken);

        ArgumentNullException.ThrowIfNull(file);

        await _repository.RemoveAsync(file, cancellationToken);
    }

    public async Task<PagedResponse<File>> SelectAsync(GetFileRequest request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetWithCountAsync(x => x.FolderId == request.FolderId, request.PerInPage, request.Page, cancellationToken);
        return new PagedResponse<File>(items, totalCount, request.Page, request.PerInPage);
    }

    public Task UpdateAsync(UpdateFileRequest request, CancellationToken cancellationToken)
    {
        return _repository.UpdateAsync(FileMapper.GetModel(request), cancellationToken);
    }
}
