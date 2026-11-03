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
        System.Linq.Expressions.Expression<Func<File, object>> orderBy = request.SortBy switch
        {
            "name" => x => x.Name,
            "created_at" => x => x.CreatedAt,
            _ => x => x.Id
        };
        var (items, totalCount) = await _repository.GetWithCountAsync(
            x => x.FolderId == request.FolderId
                && (string.IsNullOrWhiteSpace(request.Search) || x.Name.Contains(request.Search)),
            request.PerInPage,
            request.Page,
            orderBy,
            request.SortDirection == "desc",
            cancellationToken);
        return new PagedResponse<File>(items, totalCount, request.Page, request.PerInPage);
    }

    public async Task<PagedResponse<File>> SelectTrashAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetDeletedWithCountAsync(perInPage, page, cancellationToken);
        return new PagedResponse<File>(items, totalCount, page, perInPage);
    }

    public async Task<bool> RestoreAsync(long id, CancellationToken cancellationToken)
        => await _repository.RestoreAsync(id, cancellationToken) is not null;

    public Task UpdateAsync(UpdateFileRequest request, CancellationToken cancellationToken)
    {
        return _repository.UpdateAsync(FileMapper.GetModel(request), cancellationToken);
    }
}
