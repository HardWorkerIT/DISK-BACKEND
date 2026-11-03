using Common.DTOs;
using Files.Application.DTOs.Requests;
using Files.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Files.Application.Interfaces;

public interface IFolderService
{
    Task<Folder?> CreateAsync(CreateFolderRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<Folder>> SelectAsync(GetFolderRequest request, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateFolderRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(DeleteFolderRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<Folder>> SelectTrashAsync(int perInPage, int page, CancellationToken cancellationToken);
    Task<bool> RestoreAsync(long id, CancellationToken cancellationToken);
}
