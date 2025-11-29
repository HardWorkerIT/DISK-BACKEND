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
        var (items, totalCount) = await _repository.GetWithCountAsync(request.PerInPage, request.Page, cancellationToken);
        return new PagedResponse<Folder>(items, totalCount, request.Page, request.PerInPage);
    }

    public Task UpdateAsync(UpdateFolderRequest request, CancellationToken cancellationToken)
    {
        return _repository.UpdateAsync(FolderMapper.GetModel(request), cancellationToken);
    }
}
