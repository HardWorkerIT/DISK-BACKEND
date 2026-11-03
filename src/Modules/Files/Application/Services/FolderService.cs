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

namespace Files.Application.Services;

public sealed class FolderService(IDbContext context) : IFolderService
{
    public readonly IGenericRepository<Folder> _repository = new GenericRepository<Folder>(context);

    public Task<Folder?> CreateAsync(CreateFolderRequest request, CancellationToken cancellationToken)
    {
        return _repository.CreateAsync(FolderMapper.GetModel(request), cancellationToken);
    }

    public async Task DeleteAsync(DeleteFolderRequest request, CancellationToken cancellationToken)
    {
        Folder? folder = await _repository.FindByIdAsync(request.Id, cancellationToken);

        ArgumentNullException.ThrowIfNull(folder);

        await _repository.RemoveAsync(folder, cancellationToken);
    }

    public async Task<PagedResponse<Folder>> SelectAsync(GetFolderRequest request, CancellationToken cancellationToken)
    {
        System.Linq.Expressions.Expression<Func<Folder, object>> orderBy = request.SortBy switch
        {
            "name" => x => x.Name,
            _ => x => x.Id
        };
        var (items, totalCount) = await _repository.GetWithCountAsync(
            x => (!request.ParentId.HasValue || x.ParentId == request.ParentId)
                && (string.IsNullOrWhiteSpace(request.Search) || x.Name.Contains(request.Search)),
            request.PerInPage,
            request.Page,
            orderBy,
            request.SortDirection == "desc",
            cancellationToken);
        return new PagedResponse<Folder>(items, totalCount, request.Page, request.PerInPage);
    }

    public async Task<PagedResponse<Folder>> SelectTrashAsync(int perInPage, int page, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _repository.GetDeletedWithCountAsync(perInPage, page, cancellationToken);
        return new PagedResponse<Folder>(items, totalCount, page, perInPage);
    }

    public async Task<bool> RestoreAsync(long id, CancellationToken cancellationToken)
        => await _repository.RestoreAsync(id, cancellationToken) is not null;

    public Task UpdateAsync(UpdateFolderRequest request, CancellationToken cancellationToken)
    {
        return _repository.UpdateAsync(FolderMapper.GetModel(request), cancellationToken);
    }
}
